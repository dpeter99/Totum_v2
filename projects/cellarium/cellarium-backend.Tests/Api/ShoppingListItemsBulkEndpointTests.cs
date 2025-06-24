using System.Net;
using System.Net.Http.Json;
using Cellarium.Tests.Infrastructure;
using cellarium_backend.Dto;

namespace Cellarium.Tests.Api;

/// <summary>
/// Tests for the /api/shopping-list/{listId}/items bulk endpoint covering all HTTP methods.
/// Organized by functional requirements: Bulk Creation, Bulk Updates, Bulk Deletion, Authorization, Edge Cases.
/// </summary>
[Collection("API Tests")]
public class ShoppingListItemsBulkEndpointTests : ApiTestBase
{
    public ShoppingListItemsBulkEndpointTests(TestApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region POST /api/shopping-list/{listId}/items - Bulk Item Creation

    [Fact]
    public async Task POST_WithValidBulkData_Creates_AllItems()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var bulkDto = new BulkShoppingListItemCreationDto
        {
            Items = new List<BulkItemCreation>
            {
                new BulkItemCreation { Name = "Milk" },
                new BulkItemCreation { Name = "Bread" },
                new BulkItemCreation { Name = "Eggs" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.CreatedItems);
        Assert.Equal(3, result.CreatedItems.Count);
    }

    [Fact]
    public async Task POST_WithMixedValidInvalidData_ReturnsPartialSuccess()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var bulkDto = new BulkShoppingListItemCreationDto
        {
            Items = new List<BulkItemCreation>
            {
                new BulkItemCreation { Name = "Valid Item" },
                new BulkItemCreation { Name = "" }, // Invalid - empty name
                new BulkItemCreation { Name = "Another Valid" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.NotNull(result.CreatedItems);
        Assert.Equal(2, result.CreatedItems.Count);
    }

    [Fact]
    public async Task POST_ToNonExistentList_Returns404()
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();
        var bulkDto = new BulkShoppingListItemCreationDto
        {
            Items = new List<BulkItemCreation>
            {
                new BulkItemCreation { Name = "Test Item" }
            }
        };

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{nonExistentListId}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithEmptyItemsList_Returns400()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var bulkDto = new BulkShoppingListItemCreationDto
        {
            Items = new List<BulkItemCreation>()
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithoutAuthentication_Returns401()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var bulkDto = new BulkShoppingListItemCreationDto
        {
            Items = new List<BulkItemCreation>
            {
                new BulkItemCreation { Name = "Test Item" }
            }
        };

        // Act - Don't authenticate
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region PUT /api/shopping-list/{listId}/items - Bulk Item Updates

    [Fact]
    public async Task PUT_WithValidBulkUpdates_UpdatesAllItems()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        // Create some items to update
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Original Item 1");
        var item2 = await CreateShoppingListItemAsync(list.Id, userId, "Original Item 2");
        
        var bulkDto = new BulkShoppingListItemUpdateDto
        {
            Updates = new List<BulkItemUpdate>
            {
                new BulkItemUpdate
                {
                    Id = item1.id,
                    Name = "Updated Item 1",
                    IsCompleted = true
                },
                new BulkItemUpdate
                {
                    Id = item2.id,
                    Name = "Updated Item 2",
                    Quantity = 5.5m,
                    Unit = "pieces"
                }
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.UpdatedItems);
        Assert.Equal(2, result.UpdatedItems.Count);
        
        // Verify updates were applied
        var updatedItem1 = result.UpdatedItems.First(i => i.id == item1.id);
        Assert.Equal("Updated Item 1", updatedItem1.name);
        Assert.True(updatedItem1.isCompleted);
        
        var updatedItem2 = result.UpdatedItems.First(i => i.id == item2.id);
        Assert.Equal("Updated Item 2", updatedItem2.name);
        Assert.Equal(5.5m, updatedItem2.quantity);
        Assert.Equal("pieces", updatedItem2.unit);
    }

    [Fact]
    public async Task PUT_WithMixedValidInvalidIds_ReturnsPartialSuccess()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Valid Item");
        
        var bulkDto = new BulkShoppingListItemUpdateDto
        {
            Updates = new List<BulkItemUpdate>
            {
                new BulkItemUpdate
                {
                    Id = item1.id,
                    Name = "Updated Item"
                },
                new BulkItemUpdate
                {
                    Id = Guid.NewGuid().ToString(), // Non-existent item
                    Name = "Should Fail"
                }
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.NotNull(result.UpdatedItems);
        Assert.Single(result.UpdatedItems);
    }

    [Fact]
    public async Task PUT_ToNonExistentList_Returns404()
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();
        var bulkDto = new BulkShoppingListItemUpdateDto
        {
            Updates = new List<BulkItemUpdate>
            {
                new BulkItemUpdate
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test"
                }
            }
        };

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{nonExistentListId}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region DELETE /api/shopping-list/{listId}/items - Bulk Item Deletion

    [Fact]
    public async Task DELETE_WithValidIds_DeletesAllItems()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        // Create items to delete
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Item 1");
        var item2 = await CreateShoppingListItemAsync(list.Id, userId, "Item 2");
        var item3 = await CreateShoppingListItemAsync(list.Id, userId, "Item 3");
        
        var bulkDto = new BulkShoppingListItemDeleteDto
        {
            ItemIds = new List<string> { item1.id, item2.id }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/shopping-list/{list.Id}/items")
        {
            Content = JsonContent.Create(bulkDto)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Errors);
        
        // Verify items were deleted by checking remaining items
        var remainingResponse = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");
        var remainingItems = await remainingResponse.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        Assert.NotNull(remainingItems);
        Assert.Single(remainingItems);
        Assert.Equal(item3.id, remainingItems[0].id);
    }

    [Fact]
    public async Task DELETE_WithMixedValidInvalidIds_ReturnsPartialSuccess()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Valid Item");
        
        var bulkDto = new BulkShoppingListItemDeleteDto
        {
            ItemIds = new List<string> 
            { 
                item1.id, 
                Guid.NewGuid().ToString() // Non-existent item
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/shopping-list/{list.Id}/items")
        {
            Content = JsonContent.Create(bulkDto)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task DELETE_ToNonExistentList_Returns404()
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();
        var bulkDto = new BulkShoppingListItemDeleteDto
        {
            ItemIds = new List<string> { Guid.NewGuid().ToString() }
        };

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/shopping-list/{nonExistentListId}/items")
        {
            Content = JsonContent.Create(bulkDto)
        });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task BulkOperations_ToOtherUserList_Return404()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);
        var item = await CreateShoppingListItemAsync(user1List.Id, user1, "User 1 Item");
        
        var createDto = new BulkShoppingListItemCreationDto
        {
            Items = new List<BulkItemCreation>
            {
                new BulkItemCreation { Name = "Test Item" }
            }
        };
        
        var updateDto = new BulkShoppingListItemUpdateDto
        {
            Updates = new List<BulkItemUpdate>
            {
                new BulkItemUpdate { Id = item.id, Name = "Hacked" }
            }
        };
        
        var deleteDto = new BulkShoppingListItemDeleteDto
        {
            ItemIds = new List<string> { item.id }
        };

        // Act & Assert - Try as user2
        AuthenticateAs(user2);
        
        var createResponse = await Client.PostAsJsonAsync($"/api/shopping-list/{user1List.Id}/items", createDto);
        Assert.Equal(HttpStatusCode.NotFound, createResponse.StatusCode);
        
        var updateResponse = await Client.PutAsJsonAsync($"/api/shopping-list/{user1List.Id}/items", updateDto);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        
        var deleteResponse = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/shopping-list/{user1List.Id}/items")
        {
            Content = JsonContent.Create(deleteDto)
        });
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    #endregion

    #region Edge Cases and Validation

    [Fact]
    public async Task POST_WithTooManyItems_Returns400()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        // Create more than 50 items (the limit)
        var items = new List<BulkItemCreation>();
        for (int i = 0; i < 51; i++)
        {
            items.Add(new BulkItemCreation { Name = $"Item {i}" });
        }
        
        var bulkDto = new BulkShoppingListItemCreationDto { Items = items };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_WithInvalidGuidIds_ReturnsPartialFailure()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var bulkDto = new BulkShoppingListItemUpdateDto
        {
            Updates = new List<BulkItemUpdate>
            {
                new BulkItemUpdate
                {
                    Id = "invalid-guid",
                    Name = "Should Fail"
                }
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}/items", bulkDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.Contains("Invalid item ID", result.Errors[0]);
    }

    [Fact]
    public async Task DELETE_WithInvalidGuidIds_ReturnsPartialFailure()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var bulkDto = new BulkShoppingListItemDeleteDto
        {
            ItemIds = new List<string> { "invalid-guid" }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/shopping-list/{list.Id}/items")
        {
            Content = JsonContent.Create(bulkDto)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.Contains("Invalid item ID", result.Errors[0]);
    }

    #endregion
}
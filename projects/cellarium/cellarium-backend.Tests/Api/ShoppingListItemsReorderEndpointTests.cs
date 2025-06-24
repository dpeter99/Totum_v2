using System.Net;
using System.Net.Http.Json;
using Cellarium.Tests.Infrastructure;
using cellarium_backend.Dto;

namespace Cellarium.Tests.Api;

/// <summary>
/// Tests for the /api/shopping-list/{listId}/items/reorder endpoint.
/// Covers item reordering functionality, validation, authorization, and edge cases.
/// </summary>
[Collection("API Tests")]
public class ShoppingListItemsReorderEndpointTests : ApiTestBase
{
    public ShoppingListItemsReorderEndpointTests(TestApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region PATCH /api/shopping-list/{listId}/items/reorder - Item Reordering

    [Fact]
    public async Task PATCH_ReorderItems_UpdatesOrderCorrectly()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        // Create items with specific order
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Item 1");
        var item2 = await CreateShoppingListItemAsync(list.Id, userId, "Item 2");
        var item3 = await CreateShoppingListItemAsync(list.Id, userId, "Item 3");
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = item1.id, NewOrder = 3 },
                new ItemReorderDto { ItemId = item2.id, NewOrder = 1 },
                new ItemReorderDto { ItemId = item3.id, NewOrder = 2 }
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.UpdatedItems);
        Assert.Equal(3, result.UpdatedItems.Count);
        
        // Verify the order was updated correctly
        var updatedItem1 = result.UpdatedItems.First(i => i.id == item1.id);
        var updatedItem2 = result.UpdatedItems.First(i => i.id == item2.id);
        var updatedItem3 = result.UpdatedItems.First(i => i.id == item3.id);
        
        Assert.Equal(3, updatedItem1.order);
        Assert.Equal(1, updatedItem2.order);
        Assert.Equal(2, updatedItem3.order);
    }

    [Fact]
    public async Task PATCH_ReorderItems_VerifiesOrderPersistsInListRetrieval()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "First");
        var item2 = await CreateShoppingListItemAsync(list.Id, userId, "Second");
        var item3 = await CreateShoppingListItemAsync(list.Id, userId, "Third");
        
        // Reorder: item3 first, item1 second, item2 third
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = item1.id, NewOrder = 2 },
                new ItemReorderDto { ItemId = item2.id, NewOrder = 3 },
                new ItemReorderDto { ItemId = item3.id, NewOrder = 1 }
            }
        };

        // Act - Reorder
        AuthenticateAs(userId);
        var reorderResponse = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);
        reorderResponse.EnsureSuccessStatusCode();
        
        // Act - Get all items to verify ordering
        var getResponse = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");
        
        // Assert
        getResponse.EnsureSuccessStatusCode();
        var items = await getResponse.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        
        Assert.NotNull(items);
        Assert.Equal(3, items.Length);
        
        // Items should be ordered by Order property: item3(1), item1(2), item2(3)
        Assert.Equal(item3.id, items[0].id);
        Assert.Equal("Third", items[0].name);
        Assert.Equal(1, items[0].order);
        
        Assert.Equal(item1.id, items[1].id);
        Assert.Equal("First", items[1].name);
        Assert.Equal(2, items[1].order);
        
        Assert.Equal(item2.id, items[2].id);
        Assert.Equal("Second", items[2].name);
        Assert.Equal(3, items[2].order);
    }

    [Fact]
    public async Task PATCH_ReorderItems_WithMixedValidInvalidIds_ReturnsPartialSuccess()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Valid Item");
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = item1.id, NewOrder = 5 },
                new ItemReorderDto { ItemId = Guid.NewGuid().ToString(), NewOrder = 1 } // Non-existent item
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.Contains("Item not found", result.Errors[0]);
        Assert.NotNull(result.UpdatedItems);
        Assert.Single(result.UpdatedItems);
        Assert.Equal(5, result.UpdatedItems[0].order);
    }

    [Fact]
    public async Task PATCH_ReorderItems_ToNonExistentList_Returns404()
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = Guid.NewGuid().ToString(), NewOrder = 1 }
            }
        };

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{nonExistentListId}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_ReorderItems_ToOtherUserList_Returns404()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);
        var item = await CreateShoppingListItemAsync(user1List.Id, user1, "User 1 Item");
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = item.id, NewOrder = 1 }
            }
        };

        // Act - Try to reorder as user2
        AuthenticateAs(user2);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{user1List.Id}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_ReorderItems_WithoutAuthentication_Returns401()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var item = await CreateShoppingListItemAsync(list.Id, userId, "Test Item");
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = item.id, NewOrder = 1 }
            }
        };

        // Act - Don't authenticate
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_ReorderItems_WithInvalidGuid_ReturnsPartialFailure()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = "invalid-guid", NewOrder = 1 }
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);

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
    public async Task PATCH_ReorderItems_WithEmptyReordersList_Returns400()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>()
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_ReorderItems_WithZeroOrder_Works()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var item = await CreateShoppingListItemAsync(list.Id, userId, "Test Item");
        
        var reorderDto = new BulkItemReorderDto
        {
            Reorders = new List<ItemReorderDto>
            {
                new ItemReorderDto { ItemId = item.id, NewOrder = 0 }
            }
        };

        // Act
        AuthenticateAs(userId);
        var response = await Client.PatchAsJsonAsync($"/api/shopping-list/{list.Id}/items/reorder", reorderDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<BulkOperationResultDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.UpdatedItems);
        Assert.Single(result.UpdatedItems);
        Assert.Equal(0, result.UpdatedItems[0].order);
    }

    #endregion
}
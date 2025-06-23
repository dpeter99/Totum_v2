using System.Net;
using System.Net.Http.Json;
using Cellarium.Tests.Infrastructure;
using cellarium_backend.Dto;

namespace Cellarium.Tests.Api;

/// <summary>
/// Tests for shopping list business logic validation.
/// Covers advanced validation scenarios beyond basic input validation.
/// </summary>
[Collection("API Tests")]
public class ShoppingListBusinessLogicTests : ApiTestBase
{
    public ShoppingListBusinessLogicTests(TestApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region Whitespace-only Name Validation

    [Fact]
    public async Task POST_WithWhitespaceOnlyName_Returns400BadRequest()
    {
        // Arrange
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("   \t\n   ")  // Multiple types of whitespace
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("whitespace", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PUT_WithWhitespaceOnlyName_Returns400BadRequest()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        
        // Create a list with a valid name first
        AuthenticateAs(userId);
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Valid Original Name")
            .Build();
        var createResponse = await Client.PostAsJsonAsync("/api/shopping-list", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createdList = await createResponse.Content.ReadFromJsonAsync<ShoppingListDto>();
        
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("   \t   ")  // Whitespace only
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{createdList!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("whitespace", content, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Duplicate Name Validation

    [Fact]
    public async Task POST_WithDuplicateName_Returns201Created_InDevelopment()
    {
        // Note: Duplicate name validation is disabled in Development environment for testing
        // In production environment, this would return 400 BadRequest
        
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var existingListName = "My Shopping List";
        
        // Create first list
        await CreateShoppingListAsync(userId, existingListName);
        
        // Try to create second list with same name
        var duplicateDto = ShoppingListCreationDtoBuilder.Default()
            .WithName(existingListName)
            .Build();

        // Act
        AuthenticateAs(userId);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", duplicateDto);

        // Assert - Should succeed in Development environment
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(existingListName, createdList.Name);
    }

    [Fact]
    public async Task POST_WithDuplicateNameDifferentCase_Returns201Created_InDevelopment()
    {
        // Note: Duplicate name validation is disabled in Development environment for testing
        // In production environment, this would return 400 BadRequest
        
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        
        // Create first list
        await CreateShoppingListAsync(userId, "grocery list");
        
        // Try to create second list with different case
        var duplicateDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("GROCERY LIST")
            .Build();

        // Act
        AuthenticateAs(userId);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", duplicateDto);

        // Assert - Should succeed in Development environment
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal("GROCERY LIST", createdList.Name);
    }

    [Fact]
    public async Task POST_WithDuplicateNameDifferentUser_Returns201Created()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        var sameName = "Shared List Name";
        
        // Create list for user1
        await CreateShoppingListAsync(user1, sameName);
        
        // Create list for user2 with same name (should be allowed)
        var user2Dto = ShoppingListCreationDtoBuilder.Default()
            .WithName(sameName)
            .Build();

        // Act
        AuthenticateAs(user2);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", user2Dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(sameName, createdList.Name);
    }

    [Fact]
    public async Task PUT_WithDuplicateName_Returns200OK_InDevelopment()
    {
        // Note: Duplicate name validation is disabled in Development environment for testing
        // In production environment, this would return 400 BadRequest
        
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        
        // Create two lists
        var list1 = await CreateShoppingListAsync(userId, "List One");
        var list2 = await CreateShoppingListAsync(userId, "List Two");
        
        // Try to rename list2 to same name as list1
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("List One")
            .Build();

        // Act
        AuthenticateAs(userId);
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list2.Id}", updateDto);

        // Assert - Should succeed in Development environment
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        Assert.NotNull(updatedList);
        Assert.Equal("List One", updatedList.Name);
    }

    [Fact]
    public async Task PUT_WithSameName_Returns200OK()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        
        // Update with same name (should be allowed)
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Original Name")
            .WithDescription("Updated description")
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        Assert.NotNull(updatedList);
        Assert.Equal("Original Name", updatedList.Name);
        Assert.Equal("Updated description", updatedList.Description);
    }

    #endregion

    #region Soft Delete

    [Fact]
    public async Task DELETE_SoftDeletesList_DoesNotReturnInGetAll()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "To Be Deleted");

        // Act - Delete the list
        var deleteResponse = await Client.DeleteAsync($"/api/shopping-list/{list.Id}");
        
        // Assert delete was successful
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // Verify list is not returned in GetAll
        var getAllResponse = await Client.GetAsync("/api/shopping-list");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        
        var allLists = await getAllResponse.Content.ReadFromJsonAsync<ShoppingListDto[]>();
        Assert.NotNull(allLists);
        Assert.DoesNotContain(allLists, l => l.Id == list.Id);
    }

    [Fact]
    public async Task DELETE_SoftDeletesList_DoesNotReturnInGetById()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "To Be Deleted");

        // Act - Delete the list
        var deleteResponse = await Client.DeleteAsync($"/api/shopping-list/{list.Id}");
        
        // Assert delete was successful
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // Try to get the deleted list by ID
        var getResponse = await Client.GetAsync($"/api/shopping-list/{list.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DELETE_SoftDeletedList_CannotBeDeletedAgain()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "To Be Deleted");

        // Act - Delete the list twice
        var firstDeleteResponse = await Client.DeleteAsync($"/api/shopping-list/{list.Id}");
        var secondDeleteResponse = await Client.DeleteAsync($"/api/shopping-list/{list.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, firstDeleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task POST_WithSameNameAsDeletedList_Returns201Created()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var listName = "Reusable Name";
        
        // Create and delete a list
        var originalList = await CreateShoppingListAsync(userId, listName);
        var deleteResponse = await Client.DeleteAsync($"/api/shopping-list/{originalList.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // Try to create a new list with the same name
        var newDto = ShoppingListCreationDtoBuilder.Default()
            .WithName(listName)
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync("/api/shopping-list", newDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(listName, createdList.Name);
        Assert.NotEqual(originalList.Id, createdList.Id); // Should be a new list
    }

    #endregion
}
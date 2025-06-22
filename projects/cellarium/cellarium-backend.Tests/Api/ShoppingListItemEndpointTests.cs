using System.Net;
using System.Net.Http.Json;
using Cellarium.Tests.Infrastructure;
using cellarium_backend.Dto;

namespace Cellarium.Tests.Api;

/// <summary>
/// Tests for the /api/shopping-list/{listId}/item endpoint covering all HTTP methods.
/// Organized by functional requirements: Item Creation, Item Retrieval, Authorization, Edge Cases.
/// </summary>
[Collection("API Tests")]
public class ShoppingListItemEndpointTests : ApiTestBase
{
    public ShoppingListItemEndpointTests(TestApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region POST /api/shopping-list/{listId}/item - Item Creation

    [Fact]
    public async Task POST_WithValidData_Returns201Created()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var createDto = ShoppingListItemCreationDtoBuilder.Default()
            .WithName("Premium Organic Milk")
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdItem = await response.Content.ReadFromJsonAsync<ShoppingListItemDto>();
        Assert.NotNull(createdItem);
        Assert.Equal("Premium Organic Milk", createdItem.name);
        Assert.True(Guid.TryParse(createdItem.id, out _));
    }

    [Fact]
    public async Task POST_WithMinimalData_Returns201Created()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var createDto = ShoppingListItemCreationDtoBuilder.Default()
            .WithName("Bread")
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdItem = await response.Content.ReadFromJsonAsync<ShoppingListItemDto>();
        Assert.NotNull(createdItem);
        Assert.Equal("Bread", createdItem.name);
        Assert.True(Guid.TryParse(createdItem.id, out _));
    }

    [Fact]
    public async Task POST_WithoutAuthentication_Returns401()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var createDto = ShoppingListItemCreationDtoBuilder.Default().Build();

        // Act - Don't authenticate
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_ToNonExistentList_Returns404()
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();
        var createDto = ShoppingListItemCreationDtoBuilder.Default().Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{nonExistentListId}/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_ToOtherUserList_Returns404()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);
        var createDto = ShoppingListItemCreationDtoBuilder.Default().Build();

        // Act - Try to add item to user1's list as user2
        AuthenticateAs(user2);
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{user1List.Id}/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithEmptyName_StillCreates() // No validation implemented yet
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var createDto = ShoppingListItemCreationDtoBuilder.Default()
            .WithName("")
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/item", createDto);

        // Assert - Currently creates due to no validation
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(TestConstants.Items.Milk)]
    [InlineData(TestConstants.Items.Bread)]
    [InlineData(TestConstants.Items.Eggs)]
    [InlineData(TestConstants.Items.Apples)]
    public async Task POST_WithVariousItemNames_CreatesSuccessfully(string itemName)
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var createDto = ShoppingListItemCreationDtoBuilder.Default()
            .WithName(itemName)
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{list.Id}/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdItem = await response.Content.ReadFromJsonAsync<ShoppingListItemDto>();
        Assert.NotNull(createdItem);
        Assert.Equal(itemName, createdItem.name);
    }

    [Fact]
    public async Task POST_EmitsCorrectTelemetry()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        ClearTelemetry();

        // Act
        await CreateShoppingListItemAsync(list.Id, userId, "Telemetry Item");

        // Assert
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("add-shopping-list-item");
        collector.AssertSpanHasTag("add-shopping-list-item", "user.id", userId);
        collector.AssertSpanHasTag("add-shopping-list-item", "shopping_list.id", list.Id);
        collector.AssertSpanHasTag("add-shopping-list-item", "item.name", "Telemetry Item");
        collector.AssertSpanHasTag("add-shopping-list-item", "result", "success");
    }

    #endregion

    #region GET /api/shopping-list/{listId}/item - Item Retrieval

    [Fact]
    public async Task GET_ForValidList_ReturnsAllItems()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        
        var item1 = await CreateShoppingListItemAsync(list.Id, userId, "Milk");
        var item2 = await CreateShoppingListItemAsync(list.Id, userId, "Bread");
        var item3 = await CreateShoppingListItemAsync(list.Id, userId, "Eggs");

        // Act
        var response = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");

        // Assert
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        
        Assert.NotNull(items);
        Assert.Equal(3, items.Length);
        Assert.Contains(items, i => i.id == item1.id);
        Assert.Contains(items, i => i.id == item2.id);
        Assert.Contains(items, i => i.id == item3.id);
    }

    [Fact]
    public async Task GET_ForEmptyList_ReturnsEmptyArray()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);

        // Act
        var response = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");

        // Assert
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GET_ForNonExistentList_ReturnsEmptyArray() // Current behavior - returns empty instead of 404
    {
        // Arrange
        var nonExistentListId = Guid.NewGuid();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.GetAsync($"/api/shopping-list/{nonExistentListId}/item");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GET_ForOtherUserList_ReturnsEmptyArray() // Current behavior
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);
        await CreateShoppingListItemAsync(user1List.Id, user1, "User 1 Item");

        // Act - Try to get items from user1's list as user2
        AuthenticateAs(user2);
        var response = await Client.GetAsync($"/api/shopping-list/{user1List.Id}/item");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GET_WithoutAuthentication_Returns401()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);

        // Act - Don't authenticate
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_EmitsCorrectTelemetry()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        await CreateShoppingListItemAsync(list.Id, userId, "Item 1");
        await CreateShoppingListItemAsync(list.Id, userId, "Item 2");
        await CreateShoppingListItemAsync(list.Id, userId, "Item 3");
        
        ClearTelemetry();

        // Act
        AuthenticateAs(userId);
        var response = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("get-shopping-list-items");
        collector.AssertSpanHasTag("get-shopping-list-items", "user.id", userId);
        collector.AssertSpanHasTag("get-shopping-list-items", "shopping_list.id", list.Id);
        collector.AssertSpanHasTag("get-shopping-list-items", "items.count", "3");
    }

    #endregion

    #region Concurrent Operations and Performance

    [Fact]
    public async Task POST_ConcurrentItemCreation_MaintainsDataIntegrity()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Concurrent Test List");

        // Act - Create multiple items concurrently
        var tasks = new List<Task<ShoppingListItemDto>>();
        for (int i = 0; i < 10; i++)
        {
            var itemName = $"Concurrent Item {i}";
            tasks.Add(CreateShoppingListItemAsync(list.Id, userId, itemName));
        }

        var createdItems = await Task.WhenAll(tasks);

        // Assert - All items should be created successfully
        Assert.Equal(10, createdItems.Length);
        Assert.All(createdItems, item => Assert.NotNull(item));
        Assert.All(createdItems, item => Assert.True(Guid.TryParse(item.id, out _)));

        // Verify all items exist in the list
        AuthenticateAs(userId);
        var response = await Client.GetAsync($"/api/shopping-list/{list.Id}/item");
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<ShoppingListItemDto[]>();
        
        Assert.NotNull(items);
        Assert.Equal(10, items.Length);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GET_WithInvalidGuid_Returns400()
    {
        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.GetAsync("/api/shopping-list/invalid-guid/item");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithInvalidGuid_Returns400()
    {
        // Arrange
        var createDto = ShoppingListItemCreationDtoBuilder.Default().Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list/invalid-guid/item", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
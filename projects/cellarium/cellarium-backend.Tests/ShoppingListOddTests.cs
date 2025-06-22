using System.Net.Http.Json;
using cellarium_backend.Dto;

namespace cellarium_backend.Tests;

[Collection("ODD Tests")]
public class ShoppingListOddTests : ShoppingListOddBaseTest
{
    public ShoppingListOddTests(CustomApplicationFactoryWithTelemetry<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateShoppingList_WithValidRequest_EmitsCreateTelemetry()
    {
        // Arrange
        OtelTestFramework.CollectedSpans.Clear();
        const string userId = "telemetry-user-123";
        
        // Act - Create shopping list through HTTP endpoint
        await CreateValidShoppingList(_client, userId);
        
        // Assert - Verify telemetry was emitted
        var createSpan = OtelTestFramework.CollectedSpans.FirstOrDefault(
            s => s.DisplayName.Contains("create-shopping-list"));
        
        Assert.NotNull(createSpan);
        
        // Verify business context is captured
        var userIdTag = createSpan.Tags.FirstOrDefault(t => t.Key == "user.id");
        Assert.Equal(userId, userIdTag.Value);
        
        var nameTag = createSpan.Tags.FirstOrDefault(t => t.Key == "shopping_list.name");
        Assert.Equal("Test Shopping List", nameTag.Value);
        
        var resultTag = createSpan.Tags.FirstOrDefault(t => t.Key == "result");
        Assert.Equal("success", resultTag.Value);
    }

    [Fact]
    public async Task CreateShoppingList_TriggersDbOperation()
    {
        // Arrange
        OtelTestFramework.CollectedSpans.Clear();
        const string userId = "db-user-123";
        
        // Act
        await CreateValidShoppingList(_client, userId);
        
        // Assert - Verify database operation was called
        var dbSpan = OtelTestFramework.CollectedSpans.FirstOrDefault(
            s => s.DisplayName.Contains("db-add-shopping-list"));
        
        Assert.NotNull(dbSpan);
        
        var dbUserTag = dbSpan.Tags.FirstOrDefault(t => t.Key == "user.id");
        Assert.Equal(userId, dbUserTag.Value);
    }

    [Fact]
    public async Task GetShoppingLists_WithValidUser_EmitsGetTelemetry()
    {
        // Arrange
        const string userId = "get-user-123";
        await CreateValidShoppingList(_client, userId); // Create a list first
        OtelTestFramework.CollectedSpans.Clear();
        
        // Set authentication
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
        
        // Act - Get shopping lists through HTTP endpoint
        var response = await _client.GetAsync("/api/shopping-list");
        response.EnsureSuccessStatusCode();
        
        // Assert - Verify telemetry was emitted
        var getSpan = OtelTestFramework.CollectedSpans.Where(
            s => s.DisplayName == "get-shopping-lists").FirstOrDefault();
        
        Assert.NotNull(getSpan);
        
        var userIdTag = getSpan.Tags.FirstOrDefault(t => t.Key == "user.id");
        Assert.Equal(userId, userIdTag.Value);
        
        var countTag = getSpan.Tags.Where(t => t.Key == "shopping_lists.count").FirstOrDefault();
        Assert.False(countTag.Equals(default(KeyValuePair<string, string?>)), "shopping_lists.count tag should exist");
        Assert.Equal("1", countTag.Value);
    }

    [Fact]
    public async Task GetShoppingList_WithNonExistentId_EmitsNotFoundTelemetry()
    {
        // Arrange
        OtelTestFramework.CollectedSpans.Clear();
        const string userId = "notfound-user-123";
        var nonExistentId = Guid.NewGuid();
        
        // Set authentication
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
        
        // Act - Try to get non-existent shopping list
        var response = await _client.GetAsync($"/api/shopping-list/{nonExistentId}");
        
        // Assert - Should return 404
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        
        // Verify telemetry captures the not_found result
        var getSpan = OtelTestFramework.CollectedSpans.Where(
            s => s.DisplayName == "get-shopping-list").FirstOrDefault();
        
        Assert.NotNull(getSpan);
        
        var resultTag = getSpan.Tags.Where(t => t.Key == "result").FirstOrDefault();
        Assert.False(resultTag.Equals(default(KeyValuePair<string, string?>)), "result tag should exist");
        Assert.Equal("not_found", resultTag.Value);
        
        var idTag = getSpan.Tags.FirstOrDefault(t => t.Key == "shopping_list.id");
        Assert.Equal(nonExistentId.ToString(), idTag.Value);
    }

    [Fact]
    public async Task DeleteShoppingList_WithValidId_EmitsDeleteTelemetry()
    {
        // Arrange
        const string userId = "delete-user-123";
        var shoppingList = await CreateValidShoppingList(_client, userId);
        OtelTestFramework.CollectedSpans.Clear();
        
        // Set authentication
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
        
        // Act - Delete the shopping list
        var response = await _client.DeleteAsync($"/api/shopping-list/{shoppingList.Id}");
        response.EnsureSuccessStatusCode();
        
        // Assert - Verify telemetry was emitted
        var deleteSpan = OtelTestFramework.CollectedSpans.FirstOrDefault(
            s => s.DisplayName.Contains("delete-shopping-list"));
        
        Assert.NotNull(deleteSpan);
        
        var userIdTag = deleteSpan.Tags.FirstOrDefault(t => t.Key == "user.id");
        Assert.Equal(userId, userIdTag.Value);
        
        var idTag = deleteSpan.Tags.FirstOrDefault(t => t.Key == "shopping_list.id");
        Assert.Equal(shoppingList.Id.ToString(), idTag.Value);
        
        var resultTag = deleteSpan.Tags.FirstOrDefault(t => t.Key == "result");
        Assert.Equal("success", resultTag.Value);
    }

    [Fact]
    public async Task UserOperations_AreTracedWithUserContext()
    {
        // Arrange
        OtelTestFramework.CollectedSpans.Clear();
        const string userId = "context-user-123";
        
        // Act - Perform multiple operations
        var list = await CreateValidShoppingList(_client, userId);
        
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
        await _client.GetAsync("/api/shopping-list");
        await _client.DeleteAsync($"/api/shopping-list/{list.Id}");
        
        // Assert - All business operations should have user context
        var businessSpans = OtelTestFramework.CollectedSpans
            .Where(s => s.DisplayName.Contains("shopping-list") && !s.DisplayName.Contains("api/"))
            .ToList();
        
        Assert.True(businessSpans.Count >= 3, "Should have at least 3 business spans for create, get, delete");
        
        // Verify all business spans have user context
        foreach (var span in businessSpans)
        {
            var userTag = span.Tags.Where(t => t.Key == "user.id").FirstOrDefault();
            Assert.False(userTag.Equals(default(KeyValuePair<string, string?>)), $"Span {span.DisplayName} should have user.id tag");
            Assert.Equal(userId, userTag.Value);
        }
    }

    [Fact]
    public async Task DatabaseOperations_AreInstrumented()
    {
        // Arrange
        OtelTestFramework.CollectedSpans.Clear();
        const string userId = "db-ops-user-123";
        
        // Act - Create and then get a shopping list
        var list = await CreateValidShoppingList(_client, userId);
        
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);
        await _client.GetAsync($"/api/shopping-list/{list.Id}");
        
        // Assert - Should see both controller and database spans
        var dbAddSpan = OtelTestFramework.CollectedSpans.FirstOrDefault(
            s => s.DisplayName.Contains("db-add-shopping-list"));
        Assert.NotNull(dbAddSpan);
        
        var dbGetSpan = OtelTestFramework.CollectedSpans.FirstOrDefault(
            s => s.DisplayName.Contains("db-get-shopping-list"));
        Assert.NotNull(dbGetSpan);
        
        // Verify database operations have the correct context
        var addUserTag = dbAddSpan.Tags.FirstOrDefault(t => t.Key == "user.id");
        Assert.Equal(userId, addUserTag.Value);
        
        var getFoundTag = dbGetSpan.Tags.FirstOrDefault(t => t.Key == "found");
        Assert.Equal("True", getFoundTag.Value);
    }
}
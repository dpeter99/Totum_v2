using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using cellarium_backend.Dto;

namespace Cellarium.Tests.Infrastructure;

/// <summary>
/// Base class for API integration tests.
/// Provides common functionality for HTTP API testing including authentication setup,
/// telemetry collection, and helper methods for API operations.
/// </summary>
public abstract class ApiTestBase : IClassFixture<TestApplicationFactory<Program>>
{
    protected readonly TestApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    protected ApiTestBase(TestApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = CreateAuthenticatedClient();
    }

    /// <summary>
    /// Creates an HTTP client with test authentication configured
    /// </summary>
    private HttpClient CreateAuthenticatedClient()
    {
        return Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                    .AddTestAuthentication();
            });
        }).CreateClient();
    }

    /// <summary>
    /// Sets the authenticated user for subsequent requests
    /// </summary>
    protected void AuthenticateAs(string userId)
    {
        Client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(TestAuthenticationHandler.SchemeName, userId);
    }

    /// <summary>
    /// Clears collected telemetry spans for clean test state
    /// </summary>
    protected void ClearTelemetry()
    {
        TelemetryCollector.CollectedSpans.Clear();
    }

    /// <summary>
    /// Creates a shopping list via API and returns the created list
    /// </summary>
    protected async Task<ShoppingListDto> CreateShoppingListAsync(
        string? userId = null,
        string? listName = null)
    {
        // Generate unique user ID if not provided to avoid conflicts
        var actualUserId = userId ?? $"test-user-{Guid.NewGuid().ToString()[..8]}";
        AuthenticateAs(actualUserId);

        // Generate unique name to avoid duplicate validation issues
        var uniqueName = listName ?? $"{TestConstants.ShoppingLists.GroceryList} {Guid.NewGuid().ToString()[..8]}";
        
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName(uniqueName)
            .Build();

        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ShoppingListDto>() 
            ?? throw new InvalidOperationException("Failed to deserialize created shopping list");
    }

    /// <summary>
    /// Creates a shopping list item via API and returns the created item
    /// </summary>
    protected async Task<ShoppingListItemDto> CreateShoppingListItemAsync(
        string shoppingListId,
        string? userId = null,
        string? itemName = null)
    {
        // Generate unique user ID if not provided to avoid conflicts
        var actualUserId = userId ?? $"test-user-{Guid.NewGuid().ToString()[..8]}";
        AuthenticateAs(actualUserId);

        var createDto = ShoppingListItemCreationDtoBuilder.Default()
            .WithName(itemName ?? TestConstants.Items.Milk)
            .Build();

        var response = await Client.PostAsJsonAsync($"/api/shopping-list/{shoppingListId}/item", createDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ShoppingListItemDto>()
            ?? throw new InvalidOperationException("Failed to deserialize created shopping list item");
    }

    /// <summary>
    /// Gets all shopping lists for the authenticated user
    /// </summary>
    protected async Task<ShoppingListDto[]> GetShoppingListsAsync(string? userId = null)
    {
        // Generate unique user ID if not provided to avoid conflicts
        var actualUserId = userId ?? $"test-user-{Guid.NewGuid().ToString()[..8]}";
        AuthenticateAs(actualUserId);

        var response = await Client.GetAsync("/api/shopping-list");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ShoppingListDto[]>()
            ?? throw new InvalidOperationException("Failed to deserialize shopping lists");
    }

    /// <summary>
    /// Gets a specific shopping list with items
    /// </summary>
    protected async Task<ShoppingListWithItemsDto> GetShoppingListAsync(
        string listId, 
        string? userId = null)
    {
        // Generate unique user ID if not provided to avoid conflicts
        var actualUserId = userId ?? $"test-user-{Guid.NewGuid().ToString()[..8]}";
        AuthenticateAs(actualUserId);

        var response = await Client.GetAsync($"/api/shopping-list/{listId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>()
            ?? throw new InvalidOperationException("Failed to deserialize shopping list with items");
    }
}
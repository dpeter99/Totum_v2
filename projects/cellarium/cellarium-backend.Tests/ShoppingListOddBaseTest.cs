using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using cellarium_backend.Dto;

namespace cellarium_backend.Tests;

public class ShoppingListOddBaseTest : IClassFixture<CustomApplicationFactoryWithTelemetry<Program>>
{
    protected readonly CustomApplicationFactoryWithTelemetry<Program> _factory;
    protected readonly HttpClient _client;

    public ShoppingListOddBaseTest(CustomApplicationFactoryWithTelemetry<Program> factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Add test authentication scheme
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        }).CreateClient();
    }

    protected async Task<ShoppingListDto> CreateValidShoppingList(HttpClient client, string userId = "test-user-123")
    {
        // Set authentication header for test
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);

        var creationDto = new ShoppingListCreationDto { Name = "Test Shopping List" };
        var response = await client.PostAsJsonAsync("/api/shopping-list", creationDto);
        response.EnsureSuccessStatusCode();

        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        return createdList!;
    }

    protected async Task<ShoppingListItemDto> CreateValidShoppingListItem(HttpClient client, Guid shoppingListId, string userId = "test-user-123")
    {
        // Set authentication header for test
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", userId);

        var creationDto = new ShoppingListItemCreationDto { Name = "Test Item" };
        var response = await client.PostAsJsonAsync($"/api/shopping-list/{shoppingListId}/item", creationDto);
        response.EnsureSuccessStatusCode();

        var createdItem = await response.Content.ReadFromJsonAsync<ShoppingListItemDto>();
        return createdItem!;
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Test "))
        {
            var userId = authHeader.Substring("Test ".Length);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("scope", "cellarium")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("No test auth header found"));
    }
}
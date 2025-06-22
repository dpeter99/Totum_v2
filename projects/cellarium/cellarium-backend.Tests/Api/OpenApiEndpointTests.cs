using System.Net;
using System.Text.Json;
using Cellarium.Tests.Infrastructure;

namespace Cellarium.Tests.Api;

/// <summary>
/// Tests for OpenAPI documentation endpoints.
/// Verifies that OpenAPI specification is properly generated and accessible.
/// </summary>
[Collection("API Tests")]
public class OpenApiEndpointTests : ApiTestBase
{
    public OpenApiEndpointTests(TestApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GET_OpenApiSpec_Returns200WithValidJson()
    {
        // Act
        var response = await Client.GetAsync("/openapi/v1.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        // Verify it's valid JSON
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        
        // Parse to verify it's valid JSON
        var jsonDocument = JsonDocument.Parse(content);
        Assert.NotNull(jsonDocument);
    }

    [Fact]
    public async Task GET_OpenApiSpec_ContainsExpectedMetadata()
    {
        // Act
        var response = await Client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Check OpenAPI version
        var openApiVersion = jsonDocument.RootElement.GetProperty("openapi").GetString();
        Assert.StartsWith("3.0", openApiVersion);
        
        // Check info section
        var info = jsonDocument.RootElement.GetProperty("info");
        Assert.True(info.TryGetProperty("title", out _));
        Assert.True(info.TryGetProperty("version", out _));
        Assert.True(info.TryGetProperty("description", out _));
        
        // Check contact info
        var contact = info.GetProperty("contact");
        Assert.Equal("dpeter99", contact.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GET_OpenApiSpec_ContainsShoppingListEndpoints()
    {
        // Act
        var response = await Client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var paths = jsonDocument.RootElement.GetProperty("paths");
        
        // Check shopping list endpoints exist
        Assert.True(paths.TryGetProperty("/api/shopping-list", out _));
        Assert.True(paths.TryGetProperty("/api/shopping-list/{id}", out _));
        Assert.True(paths.TryGetProperty("/api/shopping-list/{shoppingListId}/item", out _));
        Assert.True(paths.TryGetProperty("/api/shopping-list/{shoppingListId}/item/{itemId}", out _));
    }

    [Fact]
    public async Task GET_OpenApiSpec_ContainsSecuritySchemes()
    {
        // Act
        var response = await Client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Check security schemes
        var components = jsonDocument.RootElement.GetProperty("components");
        var securitySchemes = components.GetProperty("securitySchemes");
        
        Assert.True(securitySchemes.TryGetProperty("UserAuth", out var userAuth));
        Assert.Equal("oauth2", userAuth.GetProperty("type").GetString());
        
        // Check OAuth flows
        var flows = userAuth.GetProperty("flows");
        Assert.True(flows.TryGetProperty("authorizationCode", out var authFlow));
        Assert.Contains("localhost:5001", authFlow.GetProperty("authorizationUrl").GetString());
        Assert.Contains("localhost:5001", authFlow.GetProperty("tokenUrl").GetString());
    }

    [Fact]
    public async Task GET_OpenApiSpec_ContainsShoppingListSchemas()
    {
        // Act
        var response = await Client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var components = jsonDocument.RootElement.GetProperty("components");
        var schemas = components.GetProperty("schemas");
        
        // Check key schemas exist
        Assert.True(schemas.TryGetProperty("ShoppingListDto", out _));
        Assert.True(schemas.TryGetProperty("ShoppingListCreationDto", out _));
        Assert.True(schemas.TryGetProperty("ShoppingListUpdateDto", out _));
        Assert.True(schemas.TryGetProperty("ShoppingListWithItemsDto", out _));
        Assert.True(schemas.TryGetProperty("ShoppingListItemDto", out _));
    }

    [Fact]
    public async Task GET_OpenApiSpec_ContainsEnhancedMetadataFields()
    {
        // Act
        var response = await Client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var components = jsonDocument.RootElement.GetProperty("components");
        var schemas = components.GetProperty("schemas");
        var shoppingListDto = schemas.GetProperty("ShoppingListDto");
        var properties = shoppingListDto.GetProperty("properties");
        
        // Check that enhanced metadata fields are documented
        Assert.True(properties.TryGetProperty("description", out var description));
        Assert.True(description.TryGetProperty("nullable", out _));
        
        Assert.True(properties.TryGetProperty("createdAt", out var createdAt));
        Assert.Equal("date-time", createdAt.GetProperty("format").GetString());
        
        Assert.True(properties.TryGetProperty("updatedAt", out var updatedAt));
        Assert.Equal("date-time", updatedAt.GetProperty("format").GetString());
    }

    [Fact]
    public async Task GET_ScalarDocumentation_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/scalar/v1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Scalar", content);
        Assert.Contains("API Reference", content);
    }

    [Fact]
    public async Task GET_ScalarDocumentation_ContainsOAuth2Configuration()
    {
        // Act
        var response = await Client.GetAsync("/scalar/v1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Check that OAuth2 configuration is present
        Assert.Contains("cellarium-client", content);
        Assert.Contains("cellarium", content);
    }
}
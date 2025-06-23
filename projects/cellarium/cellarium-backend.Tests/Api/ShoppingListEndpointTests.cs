using System.Net;
using System.Net.Http.Json;
using Cellarium.Tests.Infrastructure;
using cellarium_backend.Dto;

namespace Cellarium.Tests.Api;

/// <summary>
/// Tests for the /api/shopping-list endpoint covering all HTTP methods.
/// Organized by functional requirements: Creation, Retrieval, Updates, Deletion, Authorization.
/// </summary>
[Collection("API Tests")]
public class ShoppingListEndpointTests : ApiTestBase
{
    public ShoppingListEndpointTests(TestApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region POST /api/shopping-list - Shopping List Creation

    [Fact]
    public async Task POST_WithValidData_Returns201Created()
    {
        // Arrange
        var createDto = ShoppingListCreationDtoBuilder.GetBasic();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(createDto.Name, createdList.Name);
        Assert.True(Guid.TryParse(createdList.Id, out _));
        
        // Verify Location header
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(createdList.Id, response.Headers.Location.ToString());
    }

    [Fact]
    public async Task POST_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        var createDto = ShoppingListCreationDtoBuilder.GetBasic();

        // Act - Don't authenticate
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithEmptyName_Returns400BadRequest()
    {
        // Arrange
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("")
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert - Should now return 400 due to validation
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithWhitespaceOnlyName_Returns400BadRequest()
    {
        // Arrange
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("   ")
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithValidDescription_Returns201Created()
    {
        // Arrange
        var description = "This is a test shopping list with a description";
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Test List")
            .WithDescription(description)
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(description, createdList.Description);
    }

    [Fact]
    public async Task POST_WithNullDescription_Returns201Created()
    {
        // Arrange
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Test List")
            .WithDescription(null)
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Null(createdList.Description);
    }

    [Fact]
    public async Task POST_WithDescriptionTooLong_Returns400BadRequest()
    {
        // Arrange - Create a description longer than 500 characters
        var longDescription = new string('a', 501);
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Test List")
            .WithDescription(longDescription)
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithMaxLengthDescription_Returns201Created()
    {
        // Arrange - Test boundary condition (exactly 500 characters)
        var validDescription = new string('a', 500);
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Test List")
            .WithDescription(validDescription)
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(validDescription, createdList.Description);
    }

    [Fact]
    public async Task POST_SetsTimestampsAutomatically()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Timestamp Test List")
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        
        // Check that timestamps are set and within reasonable bounds
        Assert.True(createdList.CreatedAt > beforeCreation && createdList.CreatedAt < afterCreation);
        Assert.True(createdList.UpdatedAt > beforeCreation && createdList.UpdatedAt < afterCreation);
        
        // CreatedAt and UpdatedAt should be very close (within 1 second) for new entities
        Assert.True(Math.Abs((createdList.UpdatedAt - createdList.CreatedAt).TotalSeconds) < 1);
    }

    [Fact]
    public async Task POST_WithNameTooLong_Returns400BadRequest()
    {
        // Arrange - Create a name longer than 100 characters
        var longName = new string('a', 101);
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName(longName)
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WithValidName100Chars_Returns201Created()
    {
        // Arrange - Test boundary condition (exactly 100 characters)
        var validName = new string('a', 100);
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName(validName)
            .Build();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.PostAsJsonAsync("/api/shopping-list", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal(validName, createdList.Name);
    }

    [Fact]
    public async Task POST_CreatesWithCorrectTelemetry()
    {
        // Arrange
        ClearTelemetry();

        // Act
        await CreateShoppingListAsync(TestConstants.Users.TestUser1, "Telemetry Test List");

        // Assert
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("create-shopping-list");
        collector.AssertSpanHasTag("create-shopping-list", "user.id", TestConstants.Users.TestUser1);
        collector.AssertSpanHasTag("create-shopping-list", "shopping_list.name", "Telemetry Test List");
        collector.AssertSpanHasTag("create-shopping-list", "result", "success");
        
        collector.AssertSpanExists("db-add-shopping-list");
        collector.AssertSpanHasTag("db-add-shopping-list", "user.id", TestConstants.Users.TestUser1);
    }

    #endregion

    #region GET /api/shopping-list - Shopping List Retrieval (All Lists)

    [Fact]
    public async Task GET_AllLists_ReturnsUserLists()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list1 = await CreateShoppingListAsync(userId, "List 1");
        var list2 = await CreateShoppingListAsync(userId, "List 2");

        // Act
        var response = await Client.GetAsync("/api/shopping-list");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lists = await response.Content.ReadFromJsonAsync<ShoppingListDto[]>();
        
        Assert.NotNull(lists);
        Assert.Contains(lists, l => l.Id == list1.Id);
        Assert.Contains(lists, l => l.Id == list2.Id);
    }

    [Fact]
    public async Task GET_AllLists_ReturnsOnlyOwnLists()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1, "User 1 List");
        var user2List = await CreateShoppingListAsync(user2, "User 2 List");

        // Act - Get lists as user 1
        AuthenticateAs(user1);
        var response = await Client.GetAsync("/api/shopping-list");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lists = await response.Content.ReadFromJsonAsync<ShoppingListDto[]>();
        
        Assert.NotNull(lists);
        Assert.Contains(lists, l => l.Id == user1List.Id);
        Assert.DoesNotContain(lists, l => l.Id == user2List.Id);
    }

    [Fact]
    public async Task GET_AllLists_WithoutAuthentication_Returns401()
    {
        // Act - Don't authenticate
        var response = await Client.GetAsync("/api/shopping-list");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GET_AllLists_EmitsCorrectTelemetry()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        await CreateShoppingListAsync(userId, "List 1");
        await CreateShoppingListAsync(userId, "List 2");
        
        ClearTelemetry();

        // Act
        await GetShoppingListsAsync(userId);

        // Assert
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("get-shopping-lists");
        collector.AssertSpanHasTag("get-shopping-lists", "user.id", userId);
        
        collector.AssertSpanExists("db-get-shopping-lists");
        collector.AssertSpanHasTag("db-get-shopping-lists", "user.id", userId);
    }

    #endregion

    #region GET /api/shopping-list/{id} - Shopping List Retrieval (Single List)

    [Fact]
    public async Task GET_ByValidId_ReturnsListWithItems()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);
        var item = await CreateShoppingListItemAsync(list.Id, userId);

        // Act
        var response = await Client.GetAsync($"/api/shopping-list/{list.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var listWithItems = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        
        Assert.NotNull(listWithItems);
        Assert.Equal(list.Id, listWithItems.Id);
        Assert.Single(listWithItems.items);
        Assert.Equal(item.id, listWithItems.items.First().id);
    }

    [Fact]
    public async Task GET_ByNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.GetAsync($"/api/shopping-list/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_ByOtherUserListId_Returns404()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);

        // Act - Try to access user1's list as user2
        AuthenticateAs(user2);
        var response = await Client.GetAsync($"/api/shopping-list/{user1List.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_ByValidId_EmitsSuccessTelemetry()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Success Test List");
        
        ClearTelemetry();

        // Act
        await GetShoppingListAsync(list.Id, userId);

        // Assert
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("get-shopping-list");
        collector.AssertSpanHasTag("get-shopping-list", "user.id", userId);
        collector.AssertSpanHasTag("get-shopping-list", "shopping_list.id", list.Id);
        collector.AssertSpanHasTag("get-shopping-list", "result", "success");
        collector.AssertSpanHasTag("get-shopping-list", "shopping_list.name", "Success Test List");
        
        collector.AssertSpanExists("db-get-shopping-list");
        collector.AssertSpanHasTag("db-get-shopping-list", "found", "True");
    }

    [Fact]
    public async Task GET_ByNonExistentId_EmitsNotFoundTelemetry()
    {
        // Arrange
        ClearTelemetry();
        var userId = TestConstants.Users.TestUser1;
        var nonExistentId = Guid.NewGuid();

        // Act
        AuthenticateAs(userId);
        var response = await Client.GetAsync($"/api/shopping-list/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("get-shopping-list");
        collector.AssertSpanHasTag("get-shopping-list", "user.id", userId);
        collector.AssertSpanHasTag("get-shopping-list", "shopping_list.id", nonExistentId.ToString());
        collector.AssertSpanHasTag("get-shopping-list", "result", "not_found");
        
        collector.AssertSpanExists("db-get-shopping-list");
        collector.AssertSpanHasTag("db-get-shopping-list", "found", "False");
    }

    #endregion

    #region PUT /api/shopping-list/{id} - Shopping List Updates

    [Fact]
    public async Task PUT_WithValidData_ReturnsUpdatedList()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Updated Name")
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        
        Assert.NotNull(updatedList);
        Assert.Equal(list.Id, updatedList.Id);
        Assert.Equal("Updated Name", updatedList.Name);
    }

    [Fact]
    public async Task PUT_OtherUserList_Returns404()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Hacked Name")
            .Build();

        // Act - Try to update user1's list as user2
        AuthenticateAs(user2);
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{user1List.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        // Verify list name was not changed
        AuthenticateAs(user1);
        var verifyResponse = await GetShoppingListAsync(user1List.Id, user1);
        Assert.Equal(user1List.Name, verifyResponse.Name);
    }

    [Fact]
    public async Task PUT_WithEmptyName_Returns400BadRequest()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("")
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_WithNameTooLong_Returns400BadRequest()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        var longName = new string('b', 101);
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName(longName)
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_WithWhitespaceOnlyName_Returns400BadRequest()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("   ")
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_WithValidDescription_UpdatesDescription()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        var newDescription = "Updated description for the shopping list";
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Updated Name")
            .WithDescription(newDescription)
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        
        Assert.NotNull(updatedList);
        Assert.Equal("Updated Name", updatedList.Name);
        Assert.Equal(newDescription, updatedList.Description);
    }

    [Fact]
    public async Task PUT_WithNullDescription_ClearsDescription()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Original Name")
            .WithDescription("Original description")
            .Build();
        
        AuthenticateAs(userId);
        var createResponse = await Client.PostAsJsonAsync("/api/shopping-list", createDto);
        var createdList = await createResponse.Content.ReadFromJsonAsync<ShoppingListDto>();
        
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Updated Name")
            .WithDescription(null)
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{createdList!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        
        Assert.NotNull(updatedList);
        Assert.Equal("Updated Name", updatedList.Name);
        Assert.Null(updatedList.Description);
    }

    [Fact]
    public async Task PUT_WithDescriptionTooLong_Returns400BadRequest()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Original Name");
        var longDescription = new string('b', 501);
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Updated Name")
            .WithDescription(longDescription)
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{list.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_UpdatesTimestampButPreservesCreatedAt()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var createDto = ShoppingListCreationDtoBuilder.Default()
            .WithName("Original Name")
            .WithDescription("Original description")
            .Build();
        
        AuthenticateAs(userId);
        var createResponse = await Client.PostAsJsonAsync("/api/shopping-list", createDto);
        var createdList = await createResponse.Content.ReadFromJsonAsync<ShoppingListDto>();
        
        // Wait a moment to ensure UpdatedAt will be different
        await Task.Delay(1000);
        
        var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);
        var updateDto = ShoppingListUpdateDtoBuilder.Default()
            .WithName("Updated Name")
            .WithDescription("Updated description")
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/shopping-list/{createdList!.Id}", updateDto);
        var afterUpdate = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListWithItemsDto>();
        
        Assert.NotNull(updatedList);
        
        // CreatedAt should remain unchanged
        Assert.Equal(createdList.CreatedAt, updatedList.CreatedAt);
        
        // UpdatedAt should be updated and within reasonable bounds
        Assert.True(updatedList.UpdatedAt > beforeUpdate && updatedList.UpdatedAt < afterUpdate);
        
        // UpdatedAt should be after CreatedAt
        Assert.True(updatedList.UpdatedAt > updatedList.CreatedAt);
    }

    #endregion

    #region DELETE /api/shopping-list/{id} - Shopping List Deletion

    [Fact]
    public async Task DELETE_WithValidId_Returns204AndDeletesList()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId);

        // Act
        var response = await Client.DeleteAsync($"/api/shopping-list/{list.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify list is actually deleted
        var getResponse = await Client.GetAsync($"/api/shopping-list/{list.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DELETE_OtherUserList_Returns404()
    {
        // Arrange
        var user1 = TestConstants.Users.TestUser1;
        var user2 = TestConstants.Users.TestUser2;
        
        var user1List = await CreateShoppingListAsync(user1);

        // Act - Try to delete user1's list as user2
        AuthenticateAs(user2);
        var response = await Client.DeleteAsync($"/api/shopping-list/{user1List.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        // Verify list still exists for user1
        AuthenticateAs(user1);
        var getResponse = await Client.GetAsync($"/api/shopping-list/{user1List.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task DELETE_WithNonExistentId_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        AuthenticateAs(TestConstants.Users.TestUser1);
        var response = await Client.DeleteAsync($"/api/shopping-list/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DELETE_EmitsCorrectTelemetry()
    {
        // Arrange
        var userId = TestConstants.Users.TestUser1;
        var list = await CreateShoppingListAsync(userId, "Delete Test List");
        
        ClearTelemetry();

        // Act
        AuthenticateAs(userId);
        var response = await Client.DeleteAsync($"/api/shopping-list/{list.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var collector = TelemetryCollector.CollectedSpans;
        
        collector.AssertSpanExists("delete-shopping-list");
        collector.AssertSpanHasTag("delete-shopping-list", "user.id", userId);
        collector.AssertSpanHasTag("delete-shopping-list", "shopping_list.id", list.Id);
        collector.AssertSpanHasTag("delete-shopping-list", "result", "success");
        
        collector.AssertSpanExists("db-delete-shopping-list");
        collector.AssertSpanHasTag("db-delete-shopping-list", "result", "success");
    }

    #endregion
}
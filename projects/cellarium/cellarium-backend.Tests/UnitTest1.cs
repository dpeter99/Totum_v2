using cellarium_backend.Dto;
using cellarium_backend.Models;
using cellarium_backend.Services;
using cellarium_backend.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace cellarium_backend.Tests;

public class CellariumDbContextTests
{
    [Test]
    public void DbContext_ShouldHave_ShoppingListItemDbSet()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_ShoppingListItems")
            .Options;

        // Act & Assert
        using var context = new CellariumDbContext(options);
        
        // This should not throw an exception - DbSet<ShoppingListItem> should exist
        Assert.That(context.ShoppingListItems, Is.Not.Null, 
            "CellariumDbContext should have a ShoppingListItems DbSet property");
    }

    [Test]
    public async Task ShoppingList_WithItems_ShouldMaintainForeignKeyRelationship()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_ForeignKeyRelationship")
            .Options;

        // Act & Assert
        using var context = new CellariumDbContext(options);
        
        // Create a shopping list with items
        var shoppingList = new ShoppingList 
        { 
            Id = Guid.NewGuid(),
            Name = "Test List",
            UserId = "test-user-456"  // Add UserId to fix the test
        };
        
        var item1 = new ShoppingListItem 
        { 
            Id = Guid.NewGuid(),
            Name = "Test Item 1"
        };
        
        var item2 = new ShoppingListItem 
        { 
            Id = Guid.NewGuid(),
            Name = "Test Item 2"
        };
        
        // Add items to the shopping list
        shoppingList.Items.Add(item1);
        shoppingList.Items.Add(item2);
        
        // Save to database
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Query back with items included
        var retrievedList = await context.ShoppingList
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == shoppingList.Id);
        
        // Assert the relationship works
        Assert.That(retrievedList, Is.Not.Null, "Shopping list should be retrieved");
        Assert.That(retrievedList.Items, Has.Count.EqualTo(2), "Shopping list should have 2 items");
        Assert.That(retrievedList.Items.First().Name, Is.EqualTo("Test Item 1"), "First item should be correct");
        Assert.That(retrievedList.Items.Last().Name, Is.EqualTo("Test Item 2"), "Second item should be correct");
    }

    [Test]
    public async Task ShoppingList_ShouldBe_AssociatedWithUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UserAssociation")
            .Options;

        const string userId = "test-user-123";

        // Act & Assert
        using var context = new CellariumDbContext(options);
        
        // Create a shopping list associated with a user
        var shoppingList = new ShoppingList 
        { 
            Id = Guid.NewGuid(),
            Name = "User's Shopping List",
            UserId = userId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Try to retrieve shopping lists for this user
        // This should fail because we don't have UserId filtering yet
        var userLists = await context.ShoppingList
            .Where(sl => sl.UserId == userId)  // This should fail - UserId doesn't exist
            .ToListAsync();
        
        Assert.That(userLists, Has.Count.EqualTo(1), "User should have exactly 1 shopping list");
        Assert.That(userLists.First().Name, Is.EqualTo("User's Shopping List"), "Shopping list should be correct");
    }

    [Test]
    public async Task ShoppingListService_GetShoppingLists_ShouldFilterByUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UserFiltering")
            .Options;

        const string user1Id = "user-1";
        const string user2Id = "user-2";

        using var context = new CellariumDbContext(options);
        
        // Create shopping lists for different users
        var user1List1 = new ShoppingList { Id = Guid.NewGuid(), Name = "User 1 List 1", UserId = user1Id };
        var user1List2 = new ShoppingList { Id = Guid.NewGuid(), Name = "User 1 List 2", UserId = user1Id };
        var user2List1 = new ShoppingList { Id = Guid.NewGuid(), Name = "User 2 List 1", UserId = user2Id };
        
        context.ShoppingList.AddRange(user1List1, user1List2, user2List1);
        await context.SaveChangesAsync();
        
        // Create service instance  
        var service = new ShoppingListService(context);
        
        // Act
        var user1Lists = service.GetShoppingLists(user1Id).ToList();
        
        // Assert
        Assert.That(user1Lists, Has.Count.EqualTo(2), "User 1 should have exactly 2 shopping lists");
        Assert.That(user1Lists.All(list => list != null && list.UserId == user1Id), Is.True, "All lists should belong to user 1");
    }

    [Test]
    public async Task AddShoppingList_WithUserId_ShouldSetUserIdCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AddWithUserId")
            .Options;

        const string userId = "test-user-add";
        
        using var context = new CellariumDbContext(options);
        var service = new ShoppingListService(context);
        
        var creationDto = new ShoppingListCreationDto { Name = "Test List" };
        
        // Act
        var createdList = await service.AddShoppingList(creationDto, userId);
        
        // Assert
        Assert.That(createdList, Is.Not.Null, "Created list should not be null");
        Assert.That(createdList.UserId, Is.EqualTo(userId), "Created list should have correct UserId");
        Assert.That(createdList.Name, Is.EqualTo("Test List"), "Created list should have correct name");
        
        // Verify it was saved to database with correct UserId
        var savedList = await context.ShoppingList.FirstOrDefaultAsync(sl => sl.Id == createdList.Id);
        Assert.That(savedList, Is.Not.Null, "List should be saved to database");
        Assert.That(savedList.UserId, Is.EqualTo(userId), "Saved list should have correct UserId");
    }

    [Test]
    public async Task UserService_ShouldReturn_ActualUserIdFromClaims()
    {
        // Arrange
        var userService = new UserService();
        const string expectedUserId = "real-user-123";
        
        // Create a mock HttpContext with JWT claims
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;
        
        // Act - This should fail because UserService returns "temp" instead of actual user ID
        var user = await userService.GetUser(httpContext);
        
        // Assert
        Assert.That(user.Id, Is.EqualTo(expectedUserId), 
            "UserService should return actual user ID from claims, not 'temp'");
    }

    [Test]
    public async Task DeleteShoppingList_WithValidUserAndId_ShouldDeleteList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_DeleteList")
            .Options;

        const string userId = "delete-user-123";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the user
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "List to Delete",
            UserId = userId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Verify list exists
        var existingList = await context.ShoppingList.FindAsync(listId);
        Assert.That(existingList, Is.Not.Null, "List should exist before deletion");
        
        // Create service instance  
        var service = new ShoppingListService(context);
        
        // Act - This should fail because DeleteShoppingList method doesn't exist yet
        var deleteResult = await service.DeleteShoppingList(listId, userId);
        
        // Assert
        Assert.That(deleteResult, Is.True, "Delete operation should succeed");
        
        // Verify list was deleted
        var deletedList = await context.ShoppingList.FindAsync(listId);
        Assert.That(deletedList, Is.Null, "List should be deleted from database");
    }

    [Test]
    public async Task DeleteShoppingList_WithWrongUser_ShouldNotDeleteList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_DeleteListUnauthorized")
            .Options;

        const string ownerUserId = "owner-user-123";
        const string otherUserId = "other-user-456";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the owner
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Owner's List",
            UserId = ownerUserId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Create service instance  
        var service = new ShoppingListService(context);
        
        // Act - Try to delete with different user ID
        var deleteResult = await service.DeleteShoppingList(listId, otherUserId);
        
        // Assert
        Assert.That(deleteResult, Is.False, "Delete operation should fail for unauthorized user");
        
        // Verify list still exists
        var existingList = await context.ShoppingList.FindAsync(listId);
        Assert.That(existingList, Is.Not.Null, "List should still exist after unauthorized delete attempt");
        Assert.That(existingList.UserId, Is.EqualTo(ownerUserId), "List should still belong to original owner");
    }

    [Test]
    public async Task UpdateShoppingList_WithValidUserAndId_ShouldUpdateList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateList")
            .Options;

        const string userId = "update-user-123";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the user
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Original Name",
            UserId = userId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Create service instance  
        var service = new ShoppingListService(context);
        
        var updateDto = new ShoppingListCreationDto { Name = "Updated Name" };
        
        // Act - This should fail because UpdateShoppingList method doesn't exist yet
        var updateResult = await service.UpdateShoppingList(listId, updateDto, userId);
        
        // Assert
        Assert.That(updateResult, Is.Not.Null, "Update operation should succeed");
        Assert.That(updateResult.Name, Is.EqualTo("Updated Name"), "Name should be updated");
        
        // Verify list was updated in database
        var updatedList = await context.ShoppingList.FindAsync(listId);
        Assert.That(updatedList, Is.Not.Null, "List should still exist in database");
        Assert.That(updatedList.Name, Is.EqualTo("Updated Name"), "Database should reflect updated name");
    }

    [Test]
    public async Task UpdateShoppingList_WithWrongUser_ShouldNotUpdateList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UpdateListUnauthorized")
            .Options;

        const string ownerUserId = "owner-user-123";
        const string otherUserId = "other-user-456";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the owner
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Original Name",
            UserId = ownerUserId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Create service instance  
        var service = new ShoppingListService(context);
        
        var updateDto = new ShoppingListCreationDto { Name = "Hacked Name" };
        
        // Act - Try to update with different user ID
        var updateResult = await service.UpdateShoppingList(listId, updateDto, otherUserId);
        
        // Assert
        Assert.That(updateResult, Is.Null, "Update operation should fail for unauthorized user");
        
        // Verify list unchanged
        var unchangedList = await context.ShoppingList.FindAsync(listId);
        Assert.That(unchangedList, Is.Not.Null, "List should still exist");
        Assert.That(unchangedList.Name, Is.EqualTo("Original Name"), "Name should remain unchanged");
        Assert.That(unchangedList.UserId, Is.EqualTo(ownerUserId), "List should still belong to original owner");
    }

    [Test]
    public async Task ShoppingListItem_ShouldHave_ForeignKeyToShoppingList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_ItemForeignKey")
            .Options;

        const string userId = "item-user-123";
        var listId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Test List",
            UserId = userId
        };
        
        // Create a shopping list item - This should fail because ShoppingListItem doesn't have ShoppingListId yet
        var item = new ShoppingListItem 
        { 
            Id = itemId,
            Name = "Test Item",
            ShoppingListId = listId  // This property doesn't exist yet
        };
        
        context.ShoppingList.Add(shoppingList);
        context.ShoppingListItems.Add(item);
        await context.SaveChangesAsync();
        
        // Verify the relationship works
        var retrievedItem = await context.ShoppingListItems
            .Include(item => item.ShoppingList)
            .FirstOrDefaultAsync(item => item.Id == itemId);
        
        Assert.That(retrievedItem, Is.Not.Null, "Item should be retrieved");
        Assert.That(retrievedItem.ShoppingListId, Is.EqualTo(listId), "Item should have correct shopping list ID");
        Assert.That(retrievedItem.ShoppingList, Is.Not.Null, "Item should have shopping list navigation property");
        Assert.That(retrievedItem.ShoppingList.Name, Is.EqualTo("Test List"), "Navigation property should work");
    }

    [Test]
    public async Task AddItemToShoppingList_WithValidUser_ShouldAddItem()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AddItem")
            .Options;

        const string userId = "item-owner-123";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the user
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Test List",
            UserId = userId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Create service - This will fail because ShoppingListItemService doesn't exist yet
        var service = new ShoppingListItemService(context);
        
        var itemCreationDto = new ShoppingListItemCreationDto { Name = "Test Item" };
        
        // Act
        var addedItem = await service.AddItemToShoppingList(listId, itemCreationDto, userId);
        
        // Assert
        Assert.That(addedItem, Is.Not.Null, "Item should be added successfully");
        Assert.That(addedItem.Name, Is.EqualTo("Test Item"), "Item should have correct name");
        Assert.That(addedItem.ShoppingListId, Is.EqualTo(listId), "Item should belong to correct shopping list");
        
        // Verify it was saved to database
        var savedItem = await context.ShoppingListItems.FirstOrDefaultAsync(i => i.Id == addedItem.Id);
        Assert.That(savedItem, Is.Not.Null, "Item should be saved to database");
    }

    [Test]
    public async Task AddItemToShoppingList_WithWrongUser_ShouldNotAddItem()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AddItemUnauthorized")
            .Options;

        const string ownerUserId = "list-owner-123";
        const string otherUserId = "other-user-456";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the owner
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Owner's List",
            UserId = ownerUserId
        };
        
        context.ShoppingList.Add(shoppingList);
        await context.SaveChangesAsync();
        
        // Create service
        var service = new ShoppingListItemService(context);
        
        var itemCreationDto = new ShoppingListItemCreationDto { Name = "Unauthorized Item" };
        
        // Act - Try to add item with wrong user
        var addedItem = await service.AddItemToShoppingList(listId, itemCreationDto, otherUserId);
        
        // Assert
        Assert.That(addedItem, Is.Null, "Item should not be added for unauthorized user");
        
        // Verify no item was added to database
        var itemCount = await context.ShoppingListItems.CountAsync();
        Assert.That(itemCount, Is.EqualTo(0), "No items should be added to database");
    }

    [Test]
    public async Task GetItemsForShoppingList_WithValidUser_ShouldReturnItems()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetItems")
            .Options;

        const string userId = "get-items-user-123";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the user
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Test List",
            UserId = userId
        };
        
        // Create some items
        var item1 = new ShoppingListItem 
        { 
            Id = Guid.NewGuid(),
            Name = "Item 1",
            ShoppingListId = listId
        };
        
        var item2 = new ShoppingListItem 
        { 
            Id = Guid.NewGuid(),
            Name = "Item 2",
            ShoppingListId = listId
        };
        
        context.ShoppingList.Add(shoppingList);
        context.ShoppingListItems.AddRange(item1, item2);
        await context.SaveChangesAsync();
        
        // Create service - This will fail because GetItemsForShoppingList method doesn't exist yet
        var service = new ShoppingListItemService(context);
        
        // Act
        var items = await service.GetItemsForShoppingList(listId, userId);
        
        // Assert
        Assert.That(items, Is.Not.Null, "Items should be returned");
        Assert.That(items.Count(), Is.EqualTo(2), "Should return 2 items");
        Assert.That(items.Any(i => i.Name == "Item 1"), Is.True, "Should include Item 1");
        Assert.That(items.Any(i => i.Name == "Item 2"), Is.True, "Should include Item 2");
    }

    [Test]
    public async Task GetItemsForShoppingList_WithWrongUser_ShouldReturnEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CellariumDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_GetItemsUnauthorized")
            .Options;

        const string ownerUserId = "owner-items-123";
        const string otherUserId = "other-items-456";
        var listId = Guid.NewGuid();

        using var context = new CellariumDbContext(options);
        
        // Create a shopping list for the owner
        var shoppingList = new ShoppingList 
        { 
            Id = listId,
            Name = "Owner's List",
            UserId = ownerUserId
        };
        
        // Create some items
        var item1 = new ShoppingListItem 
        { 
            Id = Guid.NewGuid(),
            Name = "Private Item 1",
            ShoppingListId = listId
        };
        
        context.ShoppingList.Add(shoppingList);
        context.ShoppingListItems.Add(item1);
        await context.SaveChangesAsync();
        
        // Create service
        var service = new ShoppingListItemService(context);
        
        // Act - Try to get items with wrong user
        var items = await service.GetItemsForShoppingList(listId, otherUserId);
        
        // Assert
        Assert.That(items, Is.Not.Null, "Should return empty collection, not null");
        Assert.That(items.Count(), Is.EqualTo(0), "Should return no items for unauthorized user");
    }
}
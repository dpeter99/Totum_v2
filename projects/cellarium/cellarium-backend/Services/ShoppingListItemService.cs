using cellarium_backend.Dto;
using cellarium_backend.Models;

namespace cellarium_backend.Services;

public interface IShoppingListItemService
{
    Task<ShoppingListItem?> AddItemToShoppingList(Guid shoppingListId, ShoppingListItemCreationDto item, string userId);
    Task<IEnumerable<ShoppingListItem>> GetItemsForShoppingList(Guid shoppingListId, string userId);
    Task<ShoppingListItem?> GetItemById(Guid shoppingListId, Guid itemId, string userId);
    Task<ShoppingListItem?> UpdateItem(Guid shoppingListId, Guid itemId, ShoppingListItemUpdateDto updateDto, string userId);
    Task<bool> DeleteItem(Guid shoppingListId, Guid itemId, string userId);
}

public class ShoppingListItemService(CellariumDbContext db) : IShoppingListItemService
{
    public async Task<ShoppingListItem?> AddItemToShoppingList(Guid shoppingListId, ShoppingListItemCreationDto item, string userId)
    {
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            return null; // List not found or user doesn't own it
        }
        
        // Create the new item
        var newItem = item.FromDto();
        newItem.ShoppingListId = shoppingListId;
        
        var result = await db.ShoppingListItems.AddAsync(newItem);
        await db.SaveChangesAsync();
        
        return result.Entity;
    }

    public async Task<IEnumerable<ShoppingListItem>> GetItemsForShoppingList(Guid shoppingListId, string userId)
    {
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            return Enumerable.Empty<ShoppingListItem>(); // Return empty if list not found or unauthorized
        }
        
        // Return items for the shopping list
        return await Task.FromResult(db.ShoppingListItems.Where(item => item.ShoppingListId == shoppingListId).ToList());
    }

    public async Task<ShoppingListItem?> GetItemById(Guid shoppingListId, Guid itemId, string userId)
    {
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            return null; // List not found or user doesn't own it
        }
        
        // Find the specific item in the list
        var item = db.ShoppingListItems.FirstOrDefault(item => item.Id == itemId && item.ShoppingListId == shoppingListId);
        
        return await Task.FromResult(item);
    }

    public async Task<ShoppingListItem?> UpdateItem(Guid shoppingListId, Guid itemId, ShoppingListItemUpdateDto updateDto, string userId)
    {
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            return null; // List not found or user doesn't own it
        }
        
        // Find the specific item in the list
        var item = db.ShoppingListItems.FirstOrDefault(item => item.Id == itemId && item.ShoppingListId == shoppingListId);
        
        if (item == null)
        {
            return null; // Item not found
        }
        
        // Update the item with new data
        item.UpdateFromDto(updateDto);
        
        await db.SaveChangesAsync();
        
        return item;
    }

    public async Task<bool> DeleteItem(Guid shoppingListId, Guid itemId, string userId)
    {
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            return false; // List not found or user doesn't own it
        }
        
        // Find the specific item in the list
        var item = db.ShoppingListItems.FirstOrDefault(item => item.Id == itemId && item.ShoppingListId == shoppingListId);
        
        if (item == null)
        {
            return false; // Item not found
        }
        
        // Remove the item
        db.ShoppingListItems.Remove(item);
        await db.SaveChangesAsync();
        
        return true;
    }
}
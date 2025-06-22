using cellarium_backend.Dto;
using cellarium_backend.Models;
using System.Diagnostics;

namespace cellarium_backend.Services;

public interface IShoppingListService
{
    IEnumerable<ShoppingList?> GetShoppingLists();
    IEnumerable<ShoppingList?> GetShoppingLists(string userId);
    ShoppingList? GetShoppingList(Guid id);
    ShoppingList? GetShoppingList(Guid id, string userId);
    Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList);
    Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList, string userId);
    Task<bool> DeleteShoppingList(Guid id, string userId);
    Task<ShoppingList?> UpdateShoppingList(Guid id, ShoppingListUpdateDto shoppingList, string userId);
}

public class ShoppingListService(CellariumDbContext db): IShoppingListService
{
    public IEnumerable<ShoppingList?> GetShoppingLists()
    {
        return db.ShoppingList.AsQueryable().ToList();
    }

    public IEnumerable<ShoppingList?> GetShoppingLists(string userId)
    {
        using var span = ActivityHelper.Source.StartActivity("db-get-shopping-lists");
        span?.AddTag("user.id", userId);
        
        var lists = db.ShoppingList.Where(sl => sl.UserId == userId).ToList();
        span?.AddTag("lists.count", lists.Count.ToString());
        
        return lists;
    }

    public ShoppingList? GetShoppingList(Guid id)
    {
        return db.ShoppingList.Find(id);
    }

    public ShoppingList? GetShoppingList(Guid id, string userId)
    {
        using var span = ActivityHelper.Source.StartActivity("db-get-shopping-list");
        span?.AddTag("shopping_list.id", id.ToString());
        span?.AddTag("user.id", userId);
        
        var list = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId);
        if (list != null)
        {
            // Load items for this shopping list
            list.Items = db.ShoppingListItems.Where(item => item.ShoppingListId == id).ToList();
        }
        
        span?.AddTag("found", (list != null).ToString());
        
        return list;
    }

    public async Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList)
    {
        var newList = shoppingList.ToShoppingList();
        var res = await db.ShoppingList.AddAsync(newList);
        await db.SaveChangesAsync();
        return res.Entity;
    }

    public async Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList, string userId)
    {
        using var span = ActivityHelper.Source.StartActivity("db-add-shopping-list");
        span?.AddTag("shopping_list.name", shoppingList.Name);
        span?.AddTag("user.id", userId);
        
        var newList = shoppingList.ToShoppingList();
        newList.UserId = userId;  // Set the user ID
        var res = await db.ShoppingList.AddAsync(newList);
        await db.SaveChangesAsync();
        
        span?.AddTag("shopping_list.id", res.Entity.Id.ToString());
        
        return res.Entity;
    }

    public async Task<bool> DeleteShoppingList(Guid id, string userId)
    {
        using var span = ActivityHelper.Source.StartActivity("db-delete-shopping-list");
        span?.AddTag("shopping_list.id", id.ToString());
        span?.AddTag("user.id", userId);
        
        // Find the shopping list that belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            span?.AddTag("result", "not_found");
            return false; // List not found or user doesn't own it
        }
        
        db.ShoppingList.Remove(shoppingList);
        await db.SaveChangesAsync();
        
        span?.AddTag("result", "success");
        return true;
    }

    public async Task<ShoppingList?> UpdateShoppingList(Guid id, ShoppingListUpdateDto shoppingList, string userId)
    {
        // Find the shopping list that belongs to the user
        var existingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId);
        
        if (existingList == null)
        {
            return null; // List not found or user doesn't own it
        }
        
        // Update the properties using the mapper
        existingList.UpdateFromDto(shoppingList);
        
        await db.SaveChangesAsync();
        return existingList;
    }
}
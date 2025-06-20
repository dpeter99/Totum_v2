using cellarium_backend.Dto;
using cellarium_backend.Models;

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
    Task<ShoppingList?> UpdateShoppingList(Guid id, ShoppingListCreationDto shoppingList, string userId);
}

public class ShoppingListService(CellariumDbContext db): IShoppingListService
{
    public IEnumerable<ShoppingList?> GetShoppingLists()
    {
        return db.ShoppingList.AsQueryable().ToList();
    }

    public IEnumerable<ShoppingList?> GetShoppingLists(string userId)
    {
        return db.ShoppingList.Where(sl => sl.UserId == userId).ToList();
    }

    public ShoppingList? GetShoppingList(Guid id)
    {
        return db.ShoppingList.Find(id);
    }

    public ShoppingList? GetShoppingList(Guid id, string userId)
    {
        return db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId);
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
        var newList = shoppingList.ToShoppingList();
        newList.UserId = userId;  // Set the user ID
        var res = await db.ShoppingList.AddAsync(newList);
        await db.SaveChangesAsync();
        return res.Entity;
    }

    public async Task<bool> DeleteShoppingList(Guid id, string userId)
    {
        // Find the shopping list that belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            return false; // List not found or user doesn't own it
        }
        
        db.ShoppingList.Remove(shoppingList);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ShoppingList?> UpdateShoppingList(Guid id, ShoppingListCreationDto shoppingList, string userId)
    {
        // Find the shopping list that belongs to the user
        var existingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId);
        
        if (existingList == null)
        {
            return null; // List not found or user doesn't own it
        }
        
        // Update the properties
        existingList.Name = shoppingList.Name;
        
        await db.SaveChangesAsync();
        return existingList;
    }
}
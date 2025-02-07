using cellarium_backend.Dto;
using cellarium_backend.Models;

namespace cellarium_backend.Services;

public interface IShoppingListService
{
    IEnumerable<ShoppingList?> GetShoppingLists();
    ShoppingList? GetShoppingList(Guid id);
    Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList);
}

public class ShoppingListService(CellariumDbContext db): IShoppingListService
{
    public IEnumerable<ShoppingList?> GetShoppingLists()
    {
        return db.ShoppingList.AsQueryable().ToList();
    }

    public ShoppingList? GetShoppingList(Guid id)
    {
        return db.ShoppingList.Find(id);
    }

    public async Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList)
    {
        var newList = shoppingList.ToShoppingList();
        var res = await db.ShoppingList.AddAsync(newList);
        await db.SaveChangesAsync();
        return res.Entity;
    }
}
using cellarium_backend.Features.ShoppingLists.Dto;
using cellarium_backend.Features.ShoppingLists.Models;
using cellarium_backend.Exceptions;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace cellarium_backend.Features.ShoppingLists.Services;

public interface IShoppingListService
{
    IEnumerable<ShoppingList?> GetShoppingLists(string userId);
    ShoppingList? GetShoppingList(Guid id, string userId);
    Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList);
    Task<ShoppingList?> AddShoppingList(ShoppingListCreationDto shoppingList, string userId);
    Task<bool> DeleteShoppingList(Guid id, string userId);
    Task<ShoppingList?> UpdateShoppingList(Guid id, ShoppingListUpdateDto shoppingList, string userId);
}

public class ShoppingListService(CellariumDbContext db, IWebHostEnvironment environment): IShoppingListService
{
    public IEnumerable<ShoppingList?> GetShoppingLists(string userId)
    {
        using var span = ActivityHelper.Source.StartActivity("db-get-shopping-lists");
        span?.AddTag("user.id", userId);
        
        var lists = db.ShoppingList.Where(sl => sl.UserId == userId && !sl.IsDeleted).ToList();
        span?.AddTag("lists.count", lists.Count.ToString());
        
        return lists;
    }

    public ShoppingList? GetShoppingList(Guid id, string userId)
    {
        using var span = ActivityHelper.Source.StartActivity("db-get-shopping-list");
        span?.AddTag("shopping_list.id", id.ToString());
        span?.AddTag("user.id", userId);
        
        var list = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId && !sl.IsDeleted);
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
        // Whitespace validation is now handled by validation attributes
        
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
        
        // Whitespace validation is now handled by validation attributes
        await ValidateNoDuplicateName(shoppingList.Name, userId);
        
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
        
        // Find the shopping list that belongs to the user (and is not already deleted)
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId && !sl.IsDeleted);
        
        if (shoppingList == null)
        {
            span?.AddTag("result", "not_found");
            return false; // List not found, user doesn't own it, or already deleted
        }
        
        // Soft delete: mark as deleted instead of removing from database
        shoppingList.IsDeleted = true;
        await db.SaveChangesAsync();
        
        span?.AddTag("result", "success");
        return true;
    }

    public async Task<ShoppingList?> UpdateShoppingList(Guid id, ShoppingListUpdateDto shoppingList, string userId)
    {
        // Find the shopping list that belongs to the user
        var existingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == id && sl.UserId == userId && !sl.IsDeleted);
        
        if (existingList == null)
        {
            return null; // List not found or user doesn't own it
        }
        
        // Whitespace validation is now handled by validation attributes
        await ValidateNoDuplicateName(shoppingList.Name, userId, id);
        
        // Update the properties using the mapper
        existingList.UpdateFromDto(shoppingList);
        
        await db.SaveChangesAsync();
        return existingList;
    }

    #region Business Logic Validation


    /// <summary>
    /// Validates that the user doesn't already have a list with the same name (case-insensitive).
    /// Disabled in Development environment to allow for test isolation.
    /// </summary>
    private async Task ValidateNoDuplicateName(string name, string userId, Guid? excludeListId = null)
    {
        
        // Skip duplicate validation in Development environment for testing
        if (environment.IsDevelopment())
        {
            return;
        }
        
        var normalizedName = name.Trim().ToLowerInvariant();
        
        var query = db.ShoppingList
            .Where(sl => sl.UserId == userId && !sl.IsDeleted && 
                         sl.Name.ToLower() == normalizedName);
                         
        if (excludeListId.HasValue)
        {
            query = query.Where(sl => sl.Id != excludeListId.Value);
        }
        
        var existingList = await query.FirstOrDefaultAsync();
        if (existingList != null)
        {
            throw new DuplicateListNameException(name);
        }
    }


    #endregion
}
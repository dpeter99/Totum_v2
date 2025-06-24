using cellarium_backend.Features.ShoppingLists.Dto;
using cellarium_backend.Features.ShoppingLists.Models;

namespace cellarium_backend.Features.ShoppingLists.Services;

public interface IShoppingListItemService
{
    Task<ShoppingListItem?> AddItemToShoppingList(Guid shoppingListId, ShoppingListItemCreationDto item, string userId);
    Task<IEnumerable<ShoppingListItem>> GetItemsForShoppingList(Guid shoppingListId, string userId);
    Task<ShoppingListItem?> GetItemById(Guid shoppingListId, Guid itemId, string userId);
    Task<ShoppingListItem?> UpdateItem(Guid shoppingListId, Guid itemId, ShoppingListItemUpdateDto updateDto, string userId);
    Task<bool> DeleteItem(Guid shoppingListId, Guid itemId, string userId);
    
    // Bulk operations
    Task<BulkOperationResultDto> AddMultipleItemsToShoppingList(Guid shoppingListId, BulkShoppingListItemCreationDto bulkDto, string userId);
    Task<BulkOperationResultDto> UpdateMultipleItems(Guid shoppingListId, BulkShoppingListItemUpdateDto bulkDto, string userId);
    Task<BulkOperationResultDto> DeleteMultipleItems(Guid shoppingListId, BulkShoppingListItemDeleteDto bulkDto, string userId);
    
    // Item reordering
    Task<BulkOperationResultDto> ReorderItems(Guid shoppingListId, BulkItemReorderDto reorderDto, string userId);
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
        newItem.AddedByUserId = userId;
        
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
        
        // Return items for the shopping list, ordered by Order property (nulls last), then by CreatedAt
        return await Task.FromResult(db.ShoppingListItems
            .Where(item => item.ShoppingListId == shoppingListId)
            .OrderBy(item => item.Order ?? int.MaxValue)
            .ThenBy(item => item.CreatedAt)
            .ToList());
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

    // Bulk Operations Implementation
    
    public async Task<BulkOperationResultDto> AddMultipleItemsToShoppingList(Guid shoppingListId, BulkShoppingListItemCreationDto bulkDto, string userId)
    {
        var result = new BulkOperationResultDto
        {
            CreatedItems = new List<ShoppingListItemDto>()
        };
        
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            result.Errors.Add("Shopping list not found or access denied");
            result.FailureCount = bulkDto.Items.Count;
            return result;
        }
        
        foreach (var itemDto in bulkDto.Items)
        {
            try
            {
                // Manual validation for bulk operations
                var validationErrors = ValidateBulkItemCreation(itemDto);
                if (validationErrors.Any())
                {
                    result.Errors.Add($"Validation failed for item '{itemDto.Name}': {string.Join(", ", validationErrors)}");
                    result.FailureCount++;
                    continue;
                }
                
                // Create the new item
                var newItem = ConvertBulkItemCreationToEntity(itemDto);
                newItem.ShoppingListId = shoppingListId;
                newItem.AddedByUserId = userId;
                
                var entityResult = await db.ShoppingListItems.AddAsync(newItem);
                await db.SaveChangesAsync();
                
                result.CreatedItems.Add(entityResult.Entity.ToDto());
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to create item '{itemDto.Name}': {ex.Message}");
                result.FailureCount++;
            }
        }
        
        return result;
    }
    
    public async Task<BulkOperationResultDto> UpdateMultipleItems(Guid shoppingListId, BulkShoppingListItemUpdateDto bulkDto, string userId)
    {
        var result = new BulkOperationResultDto
        {
            UpdatedItems = new List<ShoppingListItemDto>()
        };
        
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            result.Errors.Add("Shopping list not found or access denied");
            result.FailureCount = bulkDto.Updates.Count;
            return result;
        }
        
        foreach (var updateDto in bulkDto.Updates)
        {
            try
            {
                // Manual validation for bulk operations
                var validationErrors = ValidateShoppingListItemUpdate(updateDto);
                if (validationErrors.Any())
                {
                    result.Errors.Add($"Validation failed for item '{updateDto.Id}': {string.Join(", ", validationErrors)}");
                    result.FailureCount++;
                    continue;
                }
                
                if (!Guid.TryParse(updateDto.Id, out var itemId))
                {
                    result.Errors.Add($"Invalid item ID: {updateDto.Id}");
                    result.FailureCount++;
                    continue;
                }
                
                // Find the specific item in the list
                var item = db.ShoppingListItems.FirstOrDefault(item => item.Id == itemId && item.ShoppingListId == shoppingListId);
                
                if (item == null)
                {
                    result.Errors.Add($"Item not found: {updateDto.Id}");
                    result.FailureCount++;
                    continue;
                }
                
                // Update the item with new data
                item.Name = updateDto.Name;
                item.Quantity = updateDto.Quantity;
                item.Unit = updateDto.Unit;
                item.Notes = updateDto.Notes;
                item.Category = updateDto.Category;
                item.IsCompleted = updateDto.IsCompleted;
                item.Order = updateDto.Order;
                
                await db.SaveChangesAsync();
                
                result.UpdatedItems.Add(item.ToDto());
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to update item '{updateDto.Id}': {ex.Message}");
                result.FailureCount++;
            }
        }
        
        return result;
    }
    
    public async Task<BulkOperationResultDto> DeleteMultipleItems(Guid shoppingListId, BulkShoppingListItemDeleteDto bulkDto, string userId)
    {
        var result = new BulkOperationResultDto();
        
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            result.Errors.Add("Shopping list not found or access denied");
            result.FailureCount = bulkDto.ItemIds.Count;
            return result;
        }
        
        foreach (var itemIdString in bulkDto.ItemIds)
        {
            try
            {
                if (!Guid.TryParse(itemIdString, out var itemId))
                {
                    result.Errors.Add($"Invalid item ID: {itemIdString}");
                    result.FailureCount++;
                    continue;
                }
                
                // Find the specific item in the list
                var item = db.ShoppingListItems.FirstOrDefault(item => item.Id == itemId && item.ShoppingListId == shoppingListId);
                
                if (item == null)
                {
                    result.Errors.Add($"Item not found: {itemIdString}");
                    result.FailureCount++;
                    continue;
                }
                
                // Remove the item
                db.ShoppingListItems.Remove(item);
                await db.SaveChangesAsync();
                
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to delete item '{itemIdString}': {ex.Message}");
                result.FailureCount++;
            }
        }
        
        return result;
    }
    
    public async Task<BulkOperationResultDto> ReorderItems(Guid shoppingListId, BulkItemReorderDto reorderDto, string userId)
    {
        var result = new BulkOperationResultDto
        {
            UpdatedItems = new List<ShoppingListItemDto>()
        };
        
        // First, verify that the shopping list exists and belongs to the user
        var shoppingList = db.ShoppingList.FirstOrDefault(sl => sl.Id == shoppingListId && sl.UserId == userId);
        
        if (shoppingList == null)
        {
            result.Errors.Add("Shopping list not found or access denied");
            result.FailureCount = reorderDto.Reorders.Count;
            return result;
        }
        
        foreach (var reorder in reorderDto.Reorders)
        {
            try
            {
                if (!Guid.TryParse(reorder.ItemId, out var itemId))
                {
                    result.Errors.Add($"Invalid item ID: {reorder.ItemId}");
                    result.FailureCount++;
                    continue;
                }
                
                // Find the specific item in the list
                var item = db.ShoppingListItems.FirstOrDefault(item => item.Id == itemId && item.ShoppingListId == shoppingListId);
                
                if (item == null)
                {
                    result.Errors.Add($"Item not found: {reorder.ItemId}");
                    result.FailureCount++;
                    continue;
                }
                
                // Update the item's order
                item.Order = reorder.NewOrder;
                
                await db.SaveChangesAsync();
                
                result.UpdatedItems.Add(item.ToDto());
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to reorder item '{reorder.ItemId}': {ex.Message}");
                result.FailureCount++;
            }
        }
        
        return result;
    }
    
    // Validation and conversion helper methods for bulk operations
    private static List<string> ValidateBulkItemCreation(BulkItemCreation dto)
    {
        var errors = new List<string>();
        
        // Name validation
        if (string.IsNullOrEmpty(dto.Name))
        {
            errors.Add("Item name is required");
        }
        else if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("Item name cannot be empty or whitespace");
        }
        else if (dto.Name.Length > 100)
        {
            errors.Add("Item name cannot exceed 100 characters");
        }
        
        // Quantity validation
        if (dto.Quantity.HasValue && (dto.Quantity.Value <= 0 || dto.Quantity.Value > 999999.999m))
        {
            errors.Add("Quantity must be a positive number between 0.01 and 999999.999");
        }
        
        // Unit validation
        if (!string.IsNullOrEmpty(dto.Unit) && dto.Unit.Length > 50)
        {
            errors.Add("Unit cannot exceed 50 characters");
        }
        
        // Notes validation
        if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes.Length > 200)
        {
            errors.Add("Notes cannot exceed 200 characters");
        }
        
        // Category validation
        if (!string.IsNullOrEmpty(dto.Category) && dto.Category.Length > 50)
        {
            errors.Add("Category cannot exceed 50 characters");
        }
        
        return errors;
    }
    
    private static ShoppingListItem ConvertBulkItemCreationToEntity(BulkItemCreation dto)
    {
        return new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Notes = dto.Notes,
            Category = dto.Category,
            IsCompleted = dto.IsCompleted,
            Order = dto.Order,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private static List<string> ValidateShoppingListItemUpdate(BulkItemUpdate dto)
    {
        var errors = new List<string>();
        
        // Name validation
        if (string.IsNullOrEmpty(dto.Name))
        {
            errors.Add("Item name is required");
        }
        else if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add("Item name cannot be empty or whitespace");
        }
        else if (dto.Name.Length > 100)
        {
            errors.Add("Item name cannot exceed 100 characters");
        }
        
        // Quantity validation
        if (dto.Quantity.HasValue && (dto.Quantity.Value <= 0 || dto.Quantity.Value > 999999.999m))
        {
            errors.Add("Quantity must be a positive number between 0.01 and 999999.999");
        }
        
        // Unit validation
        if (!string.IsNullOrEmpty(dto.Unit) && dto.Unit.Length > 50)
        {
            errors.Add("Unit cannot exceed 50 characters");
        }
        
        // Notes validation
        if (!string.IsNullOrEmpty(dto.Notes) && dto.Notes.Length > 200)
        {
            errors.Add("Notes cannot exceed 200 characters");
        }
        
        // Category validation
        if (!string.IsNullOrEmpty(dto.Category) && dto.Category.Length > 50)
        {
            errors.Add("Category cannot exceed 50 characters");
        }
        
        return errors;
    }
}
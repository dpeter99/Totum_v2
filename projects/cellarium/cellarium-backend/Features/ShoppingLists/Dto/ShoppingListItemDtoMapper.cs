using cellarium_backend.Features.ShoppingLists.Models;

namespace cellarium_backend.Features.ShoppingLists.Dto;

public static class ShoppingListItemDtoMapper
{
    public static ShoppingListItemDto ToDto(this ShoppingListItem item)
    {
        return new ShoppingListItemDto()
        {
            id = item.Id.ToString(),
            name = item.Name,
            quantity = item.Quantity,
            unit = item.Unit,
            notes = item.Notes,
            category = item.Category,
            isCompleted = item.IsCompleted,
            addedByUserId = item.AddedByUserId,
            createdAt = item.CreatedAt,
            order = item.Order
        };
    }
    
    public static ShoppingListItem FromDto(this ShoppingListItemCreationDto create)
    {
        return new ShoppingListItem()
        {
            Id = Guid.CreateVersion7(),
            Name = create.Name,
            Quantity = create.Quantity,
            Unit = create.Unit,
            Notes = create.Notes,
            Category = create.Category,
            IsCompleted = create.IsCompleted,
            Order = create.Order,
            CreatedAt = DateTime.UtcNow
            // AddedByUserId will be set in the service layer
        };
    }
    
    public static void UpdateFromDto(this ShoppingListItem item, ShoppingListItemUpdateDto update)
    {
        item.Name = update.Name;
        item.Quantity = update.Quantity;
        item.Unit = update.Unit;
        item.Notes = update.Notes;
        item.Category = update.Category;
        item.IsCompleted = update.IsCompleted;
        item.Order = update.Order;
    }
}
using cellarium_backend.Models;

namespace cellarium_backend.Dto;

public static class ShoppingListItemDtoMapper
{
    public static ShoppingListItemDto ToDto(this ShoppingListItem item)
    {
        return new ShoppingListItemDto()
        {
            id = item.Id.ToString(),
            name = item.Name,
        };
    }
    
    public static ShoppingListItem FromDto(this ShoppingListItemCreationDto create)
    {
        return new ShoppingListItem()
        {
            Id = Guid.CreateVersion7(),
            Name = create.Title,
        };
    }
}
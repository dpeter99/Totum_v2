using cellarium_backend.Features.ShoppingLists.Models;
using cellarium_backend.Shared.Models;

namespace cellarium_backend.Features.ShoppingLists.Dto;

public static class ShoppingListDtoMapper
{
    public static ShoppingListDto ToDto(this ShoppingList shoppingList)
    {
        return ShoppingListDto.Create(shoppingList);
    }
    
    public static ShoppingListWithItemsDto ToDtoWithItems(this ShoppingList shoppingList)
    {
        return new ShoppingListWithItemsDto()
        {
            Id = shoppingList.Id.ToString(),
            Name = shoppingList.Name,
            Description = shoppingList.Description,
            CreatedAt = shoppingList.CreatedAt,
            UpdatedAt = shoppingList.UpdatedAt,
            items = shoppingList.Items.Select(i => i.ToDto())
        };
    }

    public static ShoppingList ToShoppingList(this ShoppingListCreationDto shoppingList, UserId userId)
    {
        return new ShoppingList()
        {
            UserId = userId.Value,
            Name = shoppingList.Name,
            Description = shoppingList.Description,
        };
    }
    
    public static void UpdateFromDto(this ShoppingList shoppingList, ShoppingListUpdateDto updateDto)
    {
        shoppingList.Name = updateDto.Name;
        shoppingList.Description = updateDto.Description;
    }
}
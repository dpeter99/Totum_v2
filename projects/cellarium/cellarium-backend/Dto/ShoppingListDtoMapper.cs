using cellarium_backend.Controllers;
using cellarium_backend.Models;

namespace cellarium_backend.Dto;

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
            items = shoppingList.Items.Select(i => i.ToDto())
        };
    }

    public static ShoppingList ToShoppingList(this ShoppingListCreationDto shoppingList)
    {
        return new ShoppingList()
        {
            Name = shoppingList.Name,
        };
    }
}
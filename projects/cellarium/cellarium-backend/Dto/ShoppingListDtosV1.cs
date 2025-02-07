using System.ComponentModel.DataAnnotations;
using cellarium_backend.Models;

namespace cellarium_backend.Dto;

public class ShoppingListDto
{
    [Required]
    [BrandedType("ShoppingListId")]
    public string Id { get; set; }
    [Required]
    public string Name { get; set; }

    public static ShoppingListDto Create(ShoppingList shoppingList)
    {
        return new ShoppingListDto()
        {
            Id = shoppingList.Id.ToString(),
            Name = shoppingList.Name,
        };
    }
}

public class ShoppingListWithItemsDto : ShoppingListDto
{
    public IEnumerable<ShoppingListItemDto> items { get; set; }
}

public class ShoppingListCreationDto
{
    public string Name { get; set; }
}
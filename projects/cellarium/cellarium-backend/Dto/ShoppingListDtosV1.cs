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
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static ShoppingListDto Create(ShoppingList shoppingList)
    {
        return new ShoppingListDto()
        {
            Id = shoppingList.Id.ToString(),
            Name = shoppingList.Name,
            Description = shoppingList.Description,
            CreatedAt = shoppingList.CreatedAt,
            UpdatedAt = shoppingList.UpdatedAt,
        };
    }
}

public class ShoppingListWithItemsDto : ShoppingListDto
{
    public IEnumerable<ShoppingListItemDto> items { get; set; }
}

public class ShoppingListCreationDto
{
    [Required(ErrorMessage = "Shopping list name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Shopping list name must be between 1 and 100 characters")]
    public string Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class ShoppingListUpdateDto
{
    [Required(ErrorMessage = "Shopping list name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Shopping list name must be between 1 and 100 characters")]
    public string Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
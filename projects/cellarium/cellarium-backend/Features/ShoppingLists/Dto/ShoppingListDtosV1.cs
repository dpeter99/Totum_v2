using System.ComponentModel.DataAnnotations;
using cellarium_backend.Features.ShoppingLists.Models;
using cellarium_backend.Shared.Validation;

namespace cellarium_backend.Features.ShoppingLists.Dto;

public class ShoppingListDto
{
    [Required]
    [BrandedType("ShoppingListId")]
    public required string Id { get; set; }
    [Required]
    public required string Name { get; set; }
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
    public required IEnumerable<ShoppingListItemDto> items { get; set; }
}

public class ShoppingListCreationDto
{
    [Required(ErrorMessage = "Shopping list name is required")]
    [NotWhitespaceOnly]
    [StringLength(100, ErrorMessage = "Shopping list name cannot exceed 100 characters")]
    public required string Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class ShoppingListUpdateDto
{
    [Required(ErrorMessage = "Shopping list name is required")]
    [NotWhitespaceOnly]
    [StringLength(100, ErrorMessage = "Shopping list name cannot exceed 100 characters")]
    public required string Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace cellarium_backend.Features.ShoppingLists.Models;

public class ShoppingList
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public List<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}
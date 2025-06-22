using System.ComponentModel.DataAnnotations;

namespace cellarium_backend.Models;

public class ShoppingList
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    public List<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}
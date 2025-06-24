using cellarium_backend.Dto;
using System.ComponentModel.DataAnnotations;

namespace cellarium_backend.Models;

public class ShoppingListItem
{
    public Guid Id { get; set; }
    
    public Guid ShoppingListId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public decimal? Quantity { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    [MaxLength(200)]
    public string? Notes { get; set; }
    
    [MaxLength(50)]
    public string? Category { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    [Required]
    public string AddedByUserId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public int? Order { get; set; }
    
    // Navigation property
    public ShoppingList ShoppingList { get; set; } = null!;
}
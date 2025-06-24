using System.ComponentModel.DataAnnotations;
using cellarium_backend.Validation;

namespace cellarium_backend.Dto;

public class ShoppingListItemDto
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public decimal? quantity { get; set; }
    public string? unit { get; set; }
    public string? notes { get; set; }
    public string? category { get; set; }
    public bool isCompleted { get; set; }
    public string addedByUserId { get; set; } = string.Empty;
    public DateTime createdAt { get; set; }
    public int? order { get; set; }
}

public class ShoppingListItemCreationDto
{
    [Required(ErrorMessage = "Item name is required")]
    [NotWhitespaceOnly]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Range(0.01, 999999.999, ErrorMessage = "Quantity must be a positive number")]
    public decimal? Quantity { get; set; }
    
    [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
    public string? Unit { get; set; }
    
    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
    
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string? Category { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    public int? Order { get; set; }
}

public class ShoppingListItemUpdateDto
{
    [Required(ErrorMessage = "Item name is required")]
    [NotWhitespaceOnly]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Range(0.01, 999999.999, ErrorMessage = "Quantity must be a positive number")]
    public decimal? Quantity { get; set; }
    
    [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
    public string? Unit { get; set; }
    
    [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
    public string? Notes { get; set; }
    
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string? Category { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public int? Order { get; set; }
}

// Bulk Operation DTOs

public class BulkShoppingListItemCreationDto
{
    [Required(ErrorMessage = "Items list is required")]
    [MinLength(1, ErrorMessage = "At least one item must be provided")]
    [MaxLength(50, ErrorMessage = "Cannot add more than 50 items at once")]
    public List<BulkItemCreation> Items { get; set; } = new();
}

public class BulkShoppingListItemUpdateDto
{
    [Required(ErrorMessage = "Updates list is required")]
    [MinLength(1, ErrorMessage = "At least one update must be provided")]
    [MaxLength(50, ErrorMessage = "Cannot update more than 50 items at once")]
    public List<BulkItemUpdate> Updates { get; set; } = new();
}

// Simplified DTOs for bulk operations without strict validation (handled at service level)
public class BulkItemCreation
{
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public bool IsCompleted { get; set; } = false;
    public int? Order { get; set; }
}

public class BulkItemUpdate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public bool IsCompleted { get; set; }
    public int? Order { get; set; }
}

public class BulkShoppingListItemDeleteDto
{
    [Required(ErrorMessage = "Item IDs list is required")]
    [MinLength(1, ErrorMessage = "At least one item ID must be provided")]
    [MaxLength(50, ErrorMessage = "Cannot delete more than 50 items at once")]
    public List<string> ItemIds { get; set; } = new();
}

public class BulkOperationResultDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<ShoppingListItemDto>? CreatedItems { get; set; }
    public List<ShoppingListItemDto>? UpdatedItems { get; set; }
}

// Item Reordering DTOs
public class ItemReorderDto
{
    [Required(ErrorMessage = "Item ID is required")]
    public string ItemId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "New order position is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Order must be a non-negative number")]
    public int NewOrder { get; set; }
}

public class BulkItemReorderDto
{
    [Required(ErrorMessage = "Reorders list is required")]
    [MinLength(1, ErrorMessage = "At least one reorder must be provided")]
    [MaxLength(50, ErrorMessage = "Cannot reorder more than 50 items at once")]
    public List<ItemReorderDto> Reorders { get; set; } = new();
}
using cellarium_backend.Dto;

namespace cellarium_backend.Models;

public class ShoppingListItem
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public Guid ShoppingListId { get; set; }
    
    public ShoppingList ShoppingList { get; set; }
}
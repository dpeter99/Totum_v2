namespace cellarium_backend.Models;

public class ShoppingList
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public List<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}
using Microsoft.AspNetCore.Identity;

namespace cellarium_backend.Models;

public class User
{
    public string Id { get; set; }
    
    public List<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
}
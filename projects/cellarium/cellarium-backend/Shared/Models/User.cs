using Microsoft.AspNetCore.Identity;
using cellarium_backend.Features.ShoppingLists.Models;

namespace cellarium_backend.Shared.Models;

public class User
{
    public string Id { get; set; }
    
    public List<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
}
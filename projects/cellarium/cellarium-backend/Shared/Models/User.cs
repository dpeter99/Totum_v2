using Microsoft.AspNetCore.Identity;
using cellarium_backend.Features.ShoppingLists.Models;

namespace cellarium_backend.Shared.Models;

public record struct UserId (string Value)
{
}

public class User
{
    public required string Id { get; set; }
    
    public List<ShoppingList> ShoppingLists { get; set; } = new List<ShoppingList>();
}
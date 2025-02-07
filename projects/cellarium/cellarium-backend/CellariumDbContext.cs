using cellarium_backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cellarium_backend;

public class CellariumDbContext: DbContext
{
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<ShoppingList?> ShoppingList { get; set; }
    
    public CellariumDbContext(DbContextOptions options) : base(options)
    {
    }
}
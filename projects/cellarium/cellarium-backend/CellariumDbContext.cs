using cellarium_backend.Features.ShoppingLists.Models;
using cellarium_backend.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace cellarium_backend;

public class CellariumDbContext: DbContext
{
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<ShoppingList> ShoppingList { get; set; }
    
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
    
    public CellariumDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure ShoppingList timestamps
        modelBuilder.Entity<ShoppingList>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        modelBuilder.Entity<ShoppingList>()
            .Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        // Configure ShoppingListItem
        modelBuilder.Entity<ShoppingListItem>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        modelBuilder.Entity<ShoppingListItem>()
            .Property(e => e.IsCompleted)
            .HasDefaultValue(false);
            
        // Configure decimal precision for Quantity
        modelBuilder.Entity<ShoppingListItem>()
            .Property(e => e.Quantity)
            .HasPrecision(10, 3); // Up to 9999999.999
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        // Handle ShoppingList timestamps
        var shoppingListEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is ShoppingList && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in shoppingListEntries)
        {
            var entity = (ShoppingList)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                // Prevent CreatedAt from being modified
                entry.Property(nameof(entity.CreatedAt)).IsModified = false;
            }
        }
        
        // Handle ShoppingListItem timestamps
        var itemEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is ShoppingListItem && e.State == EntityState.Added);

        foreach (var entry in itemEntries)
        {
            var entity = (ShoppingListItem)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
using cellarium_backend.Dto;

namespace Cellarium.Tests.Infrastructure;

/// <summary>
/// Builder pattern for creating consistent test data objects.
/// Provides fluent API for constructing test DTOs with sensible defaults.
/// </summary>
public class ShoppingListCreationDtoBuilder
{
    private string _name = "Test Shopping List";
    private string? _description = null;

    public ShoppingListCreationDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ShoppingListCreationDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ShoppingListCreationDto Build()
    {
        return new ShoppingListCreationDto { Name = _name, Description = _description };
    }

    public static ShoppingListCreationDtoBuilder Default() => new();
    
    public static ShoppingListCreationDto GetBasic() => Default().WithName("Test List").Build();
}

/// <summary>
/// Builder for shopping list update DTOs
/// </summary>
public class ShoppingListUpdateDtoBuilder
{
    private string _name = "Updated Shopping List";
    private string? _description = null;

    public ShoppingListUpdateDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ShoppingListUpdateDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ShoppingListUpdateDto Build()
    {
        return new ShoppingListUpdateDto { Name = _name, Description = _description };
    }

    public static ShoppingListUpdateDtoBuilder Default() => new();
}

/// <summary>
/// Builder for shopping list item creation DTOs
/// </summary>
public class ShoppingListItemCreationDtoBuilder
{
    private string _name = "Test Item";

    public ShoppingListItemCreationDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ShoppingListItemCreationDto Build()
    {
        return new ShoppingListItemCreationDto
        {
            Name = _name
        };
    }

    public static ShoppingListItemCreationDtoBuilder Default() => new();
}

/// <summary>
/// Builder for shopping list item update DTOs
/// </summary>
public class ShoppingListItemUpdateDtoBuilder
{
    private string _name = "Updated Item";

    public ShoppingListItemUpdateDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ShoppingListItemUpdateDto Build()
    {
        return new ShoppingListItemUpdateDto
        {
            Name = _name
        };
    }

    public static ShoppingListItemUpdateDtoBuilder Default() => new();
}

/// <summary>
/// Constants for common test values
/// </summary>
public static class TestConstants
{
    public static class Users
    {
        public const string TestUser1 = "test-user-1";
        public const string TestUser2 = "test-user-2"; 
        public const string AdminUser = "admin-user";
    }

    public static class ShoppingLists
    {
        public const string GroceryList = "Grocery List";
        public const string HardwareStore = "Hardware Store";
        public const string WeeklyEssentials = "Weekly Essentials";
    }

    public static class Items
    {
        public const string Milk = "Milk";
        public const string Bread = "Bread";
        public const string Eggs = "Eggs";
        public const string Apples = "Apples";
    }
}
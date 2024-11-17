using cellarium_backend.Dto;

namespace cellarium_backend.Models;

public class ShoppingListItem
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }

    public static ShoppingListItem FromDto(ShoppingListItemCreation create)
    {
        return new ShoppingListItem()
        {
            Id = Guid.CreateVersion7(),
            Title = create.Title,
        };
    }
}
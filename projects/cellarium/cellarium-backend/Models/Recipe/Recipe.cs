namespace cellarium_backend.Models.Recipe;

public class Recipe
{
    public Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required string Image { get; set; }
    
    public DateTime Created { get; set; }
    
    
    
    public required List<Ingredient> Ingredients { get; set; }
}

public class Ingredient
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public int Quantity { get; set; }
    public required string Unit { get; set; }
}
namespace cellarium_backend.Models.Recipe;

public class Recipe
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string Image { get; set; }
    
    public DateTime Created { get; set; }
    
    
    
    public List<Ingredient> Ingredients { get; set; }
}

public class Ingredient
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public int Quantity { get; set; }
    public string Unit { get; set; }
}
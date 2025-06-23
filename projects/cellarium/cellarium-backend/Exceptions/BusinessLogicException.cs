namespace cellarium_backend.Exceptions;

/// <summary>
/// Base exception for business logic validation failures.
/// </summary>
public abstract class BusinessLogicException : Exception
{
    protected BusinessLogicException(string message) : base(message)
    {
    }
    
    protected BusinessLogicException(string message, Exception innerException) : base(message, innerException)
    {
    }
}


/// <summary>
/// Exception thrown when attempting to create a shopping list with a duplicate name.
/// </summary>
public class DuplicateListNameException : BusinessLogicException
{
    public string ListName { get; }
    
    public DuplicateListNameException(string listName) 
        : base($"A shopping list with the name '{listName}' already exists for this user.")
    {
        ListName = listName;
    }
}


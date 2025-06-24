using System.ComponentModel.DataAnnotations;

namespace cellarium_backend.Shared.Validation;

/// <summary>
/// Validation attribute that ensures a string is not composed entirely of whitespace characters.
/// This works alongside [Required] to provide additional validation beyond null/empty checks.
/// </summary>
public class NotWhitespaceOnlyAttribute : ValidationAttribute
{
    public NotWhitespaceOnlyAttribute() : base("The field cannot contain only whitespace characters.")
    {
    }

    public override bool IsValid(object? value)
    {
        // If value is null or empty, let [Required] handle it
        if (value == null || (value is string str && string.IsNullOrEmpty(str)))
        {
            return true;
        }

        // Check if the string contains only whitespace
        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue) && stringValue.Trim().Length > 0;
        }

        return true;
    }
}
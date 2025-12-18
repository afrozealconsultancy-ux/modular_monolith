namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Interface for business rules in the domain.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Gets the error message when the rule is broken.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Checks if the business rule is broken.
    /// </summary>
    /// <returns>True if the rule is broken; otherwise, false.</returns>
    bool IsBroken();
}

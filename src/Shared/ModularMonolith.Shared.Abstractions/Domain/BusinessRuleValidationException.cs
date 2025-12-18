namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }

    public override string ToString()
    {
        return $"{BrokenRule.GetType().Name}: {BrokenRule.Message}";
    }
}

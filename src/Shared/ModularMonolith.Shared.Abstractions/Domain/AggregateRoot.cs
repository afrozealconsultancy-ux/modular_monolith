namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Base class for aggregate roots with Guid identifiers.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot() : base(Guid.NewGuid())
    {
    }
}

/// <summary>
/// Base class for aggregate roots with custom identifier types.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Raises a domain event from this aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Checks a business rule and throws an exception if it's broken.
    /// </summary>
    /// <param name="rule">The business rule to check.</param>
    /// <exception cref="BusinessRuleValidationException">Thrown when the rule is broken.</exception>
    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}

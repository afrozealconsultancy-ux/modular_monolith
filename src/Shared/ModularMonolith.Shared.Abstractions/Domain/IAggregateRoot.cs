namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Marker interface for aggregate roots in DDD.
/// Aggregate roots are the entry points for accessing entities within an aggregate.
/// </summary>
public interface IAggregateRoot : IEntity
{
    /// <summary>
    /// Gets the domain events that have been raised by this aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// </summary>
    void ClearDomainEvents();
}

/// <summary>
/// Marker interface for aggregate roots with a specific ID type.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public interface IAggregateRoot<out TId> : IAggregateRoot, IEntity<TId> where TId : notnull
{
}

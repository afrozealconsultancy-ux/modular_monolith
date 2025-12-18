namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Base interface for all entities in the domain.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Base interface for entities with a specific ID type.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<out TId> : IEntity where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    new TId Id { get; }
}

using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Shared.Abstractions.Persistence;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
/// <typeparam name="TEntity">The type of the aggregate root.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an entity from the repository.
    /// </summary>
    void Remove(TEntity entity);
}

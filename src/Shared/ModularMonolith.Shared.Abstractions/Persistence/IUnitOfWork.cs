namespace ModularMonolith.Shared.Abstractions.Persistence;

/// <summary>
/// Interface for the Unit of Work pattern.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

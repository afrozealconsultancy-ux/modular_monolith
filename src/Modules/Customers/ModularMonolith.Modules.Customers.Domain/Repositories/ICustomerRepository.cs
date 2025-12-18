using ModularMonolith.Shared.Abstractions.Persistence;
using ModularMonolith.Modules.Customers.Domain.Entities;

namespace ModularMonolith.Modules.Customers.Domain.Repositories;

/// <summary>
/// Repository interface for Customer aggregate.
/// </summary>
public interface ICustomerRepository : IRepository<Customer, Guid>
{
    /// <summary>
    /// Gets a customer by email within current tenant.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer with the email already exists within current tenant.
    /// </summary>
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
}

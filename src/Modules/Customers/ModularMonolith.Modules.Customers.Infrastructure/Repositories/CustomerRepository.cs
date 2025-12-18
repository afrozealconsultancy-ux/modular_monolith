using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Infrastructure.Persistence;
using ModularMonolith.Modules.Customers.Domain.Entities;
using ModularMonolith.Modules.Customers.Domain.Repositories;
using ModularMonolith.Modules.Customers.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Customers.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Customer aggregate.
/// </summary>
internal sealed class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
{
    private readonly CustomersDbContext _dbContext;

    public CustomerRepository(CustomersDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        // Tenant filter applied automatically by BaseDbContext
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AnyAsync(c => c.Email == email, cancellationToken);
        // Tenant filter applied automatically
    }

    public override async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}

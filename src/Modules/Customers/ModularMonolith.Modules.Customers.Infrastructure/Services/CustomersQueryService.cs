using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Abstractions.Common;
using ModularMonolith.Modules.Customers.Application.Queries.GetCustomers;
using ModularMonolith.Modules.Customers.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Customers.Infrastructure.Services;

/// <summary>
/// Query service for efficient customer queries.
/// Uses projections and pagination.
/// </summary>
internal sealed class CustomersQueryService : ICustomersQueryService
{
    private readonly CustomersDbContext _dbContext;

    public CustomersQueryService(CustomersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedList<CustomerListItemDto>> GetCustomersAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.AsQueryable();
        // Tenant filter applied automatically

        // Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                c.Email.Contains(searchTerm));
        }

        // Count total
        var totalCount = await query.CountAsync(cancellationToken);

        // Paginate and project
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerListItemDto(
                c.Id,
                c.Name,
                c.Email,
                c.PhoneNumber,
                (int)c.Status,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedList<CustomerListItemDto>(items, page, pageSize, totalCount);
    }
}

using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Abstractions.Tenancy;
using ModularMonolith.Shared.Abstractions.Time;
using ModularMonolith.Shared.Infrastructure.Persistence;
using ModularMonolith.Modules.Customers.Domain.Entities;
using ModularMonolith.Modules.Customers.Domain.ValueObjects;

namespace ModularMonolith.Modules.Customers.Infrastructure.Persistence;

/// <summary>
/// DbContext for Customers module with separate schema.
/// </summary>
public sealed class CustomersDbContext : BaseDbContext
{
    public CustomersDbContext(
        DbContextOptions<CustomersDbContext> options,
        ITenantContext tenantContext,
        IDateTimeProvider dateTimeProvider)
        : base(options, tenantContext, dateTimeProvider)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set schema for this module
        modelBuilder.HasDefaultSchema("customers");

        base.OnModelCreating(modelBuilder);
    }
}

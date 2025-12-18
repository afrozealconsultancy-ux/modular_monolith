using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Abstractions.Domain;
using ModularMonolith.Shared.Abstractions.Tenancy;
using ModularMonolith.Shared.Abstractions.Time;
using System.Reflection;

namespace ModularMonolith.Shared.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for all modules with tenant filtering and audit support.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    protected BaseDbContext(
        DbContextOptions options,
        ITenantContext tenantContext,
        IDateTimeProvider dateTimeProvider) : base(options)
    {
        _tenantContext = tenantContext;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from the module's assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // Apply tenant query filters to all IHasTenant entities
        ApplyTenantQueryFilters(modelBuilder);

        // Apply soft delete query filters
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IHasTenant.TenantId));
                var tenantId = System.Linq.Expressions.Expression.Constant(_tenantContext.TenantId);
                var equals = System.Linq.Expressions.Expression.Equal(property, tenantId);
                var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }
    }

    private void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        // We'll add ISoftDelete interface later if needed
        // For now, keeping it simple
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set TenantId for new tenant entities
        var tenantEntries = ChangeTracker.Entries<IHasTenant>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in tenantEntries)
        {
            if (_tenantContext.TenantId.HasValue)
            {
                // Use reflection to set TenantId since it's typically protected init
                var property = entry.Property(nameof(IHasTenant.TenantId));
                if (property.CurrentValue == Guid.Empty)
                {
                    property.CurrentValue = _tenantContext.TenantId.Value;
                }
            }
        }

        // Publish domain events before saving
        var domainEvents = ChangeTracker.Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Domain events should be dispatched via MediatR
        // We'll handle this in the module-specific DbContext

        return result;
    }
}

using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Shared.Abstractions.Tenancy;

/// <summary>
/// Base class for tenant-scoped entities with Guid identifiers.
/// </summary>
public abstract class TenantEntity : TenantEntity<Guid>
{
    protected TenantEntity(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    protected TenantEntity(Guid tenantId) : base(Guid.NewGuid(), tenantId)
    {
    }
}

/// <summary>
/// Base class for tenant-scoped entities with custom identifier types.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class TenantEntity<TId> : Entity<TId>, IHasTenant where TId : notnull
{
    protected TenantEntity(TId id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Gets the tenant identifier this entity belongs to.
    /// </summary>
    public Guid TenantId { get; protected init; }
}

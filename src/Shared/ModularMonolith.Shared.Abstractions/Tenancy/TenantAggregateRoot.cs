using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Shared.Abstractions.Tenancy;

/// <summary>
/// Base class for tenant-scoped aggregate roots with Guid identifiers.
/// </summary>
public abstract class TenantAggregateRoot : TenantAggregateRoot<Guid>
{
    protected TenantAggregateRoot(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    protected TenantAggregateRoot(Guid tenantId) : base(Guid.NewGuid(), tenantId)
    {
    }
}

/// <summary>
/// Base class for tenant-scoped aggregate roots with custom identifier types.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public abstract class TenantAggregateRoot<TId> : AggregateRoot<TId>, IHasTenant where TId : notnull
{
    protected TenantAggregateRoot(TId id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Gets the tenant identifier this aggregate belongs to.
    /// </summary>
    public Guid TenantId { get; protected init; }
}

namespace ModularMonolith.Shared.Abstractions.Tenancy;

/// <summary>
/// Interface for entities that belong to a tenant.
/// </summary>
public interface IHasTenant
{
    /// <summary>
    /// Gets the tenant identifier this entity belongs to.
    /// </summary>
    Guid TenantId { get; }
}

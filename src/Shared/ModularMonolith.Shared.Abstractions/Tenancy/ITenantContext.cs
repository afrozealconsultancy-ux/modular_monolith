namespace ModularMonolith.Shared.Abstractions.Tenancy;

/// <summary>
/// Interface for accessing the current tenant context.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the unique identifier of the current tenant.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the name of the current tenant.
    /// </summary>
    string? TenantName { get; }

    /// <summary>
    /// Gets a value indicating whether the current context has a tenant.
    /// </summary>
    bool HasTenant => TenantId.HasValue;

    /// <summary>
    /// Gets a value indicating whether the current user is staff (not tenant-scoped).
    /// </summary>
    bool IsStaff { get; }
}

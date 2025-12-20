namespace ModularMonolith.Modules.Notifications.Application.Services;

/// <summary>
/// Service for resolving user notification addresses (email, phone, etc.).
/// This would typically integrate with the Identity module.
/// </summary>
public interface IUserNotificationService
{
    /// <summary>
    /// Gets the notification address (email or phone) for a specific user.
    /// </summary>
    Task<string?> GetUserNotificationAddressAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users in a specific tenant with their notification addresses.
    /// </summary>
    Task<IReadOnlyList<(Guid UserId, string Address)>> GetTenantUsersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users across all tenants with their notification addresses.
    /// Used for AllTenants broadcast scope.
    /// </summary>
    Task<IReadOnlyList<(Guid UserId, Guid TenantId, string Address)>> GetAllUsersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users from multiple tenants.
    /// </summary>
    Task<IReadOnlyList<(Guid UserId, Guid TenantId, string Address)>> GetUsersFromTenantsAsync(
        IEnumerable<Guid> tenantIds,
        CancellationToken cancellationToken = default);
}

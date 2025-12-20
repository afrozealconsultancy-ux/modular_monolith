using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.Notifications.Application.Services;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Services;

/// <summary>
/// Stub implementation of IUserNotificationService.
/// TODO: Integrate with Identity module to fetch real user data.
/// </summary>
internal sealed class UserNotificationService : IUserNotificationService
{
    private readonly ILogger<UserNotificationService> _logger;

    public UserNotificationService(ILogger<UserNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<string?> GetUserNotificationAddressAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Query Identity module for user's email or phone number
        _logger.LogWarning(
            "UserNotificationService.GetUserNotificationAddressAsync is a stub implementation. " +
            "UserId: {UserId}, TenantId: {TenantId}",
            userId,
            tenantId);

        // Stub: Return a fake email
        return Task.FromResult<string?>($"user-{userId}@example.com");
    }

    public Task<IReadOnlyList<(Guid UserId, string Address)>> GetTenantUsersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Query Identity module for all users in the tenant
        _logger.LogWarning(
            "UserNotificationService.GetTenantUsersAsync is a stub implementation. " +
            "TenantId: {TenantId}",
            tenantId);

        // Stub: Return empty list
        var result = new List<(Guid UserId, string Address)>();
        return Task.FromResult<IReadOnlyList<(Guid UserId, string Address)>>(result);
    }

    public Task<IReadOnlyList<(Guid UserId, Guid TenantId, string Address)>> GetAllUsersAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: Query Identity module for all users across all tenants
        _logger.LogWarning(
            "UserNotificationService.GetAllUsersAsync is a stub implementation.");

        // Stub: Return empty list
        var result = new List<(Guid UserId, Guid TenantId, string Address)>();
        return Task.FromResult<IReadOnlyList<(Guid UserId, Guid TenantId, string Address)>>(result);
    }

    public Task<IReadOnlyList<(Guid UserId, Guid TenantId, string Address)>> GetUsersFromTenantsAsync(
        IEnumerable<Guid> tenantIds,
        CancellationToken cancellationToken = default)
    {
        // TODO: Query Identity module for all users in the specified tenants
        _logger.LogWarning(
            "UserNotificationService.GetUsersFromTenantsAsync is a stub implementation. " +
            "TenantIds: {TenantIds}",
            string.Join(", ", tenantIds));

        // Stub: Return empty list
        var result = new List<(Guid UserId, Guid TenantId, string Address)>();
        return Task.FromResult<IReadOnlyList<(Guid UserId, Guid TenantId, string Address)>>(result);
    }
}

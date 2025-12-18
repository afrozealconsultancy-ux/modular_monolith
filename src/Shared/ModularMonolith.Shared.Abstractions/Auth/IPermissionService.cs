namespace ModularMonolith.Shared.Abstractions.Auth;

/// <summary>
/// Interface for checking user permissions.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permissions for a user in a tenant.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier (null for staff users).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of permissions.</returns>
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(
        Guid userId,
        Guid? tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier (null for staff users).</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    Task<bool> HasPermissionAsync(
        Guid userId,
        Guid? tenantId,
        string permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all specified permissions.
    /// </summary>
    Task<bool> HasAllPermissionsAsync(
        Guid userId,
        Guid? tenantId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    Task<bool> HasAnyPermissionAsync(
        Guid userId,
        Guid? tenantId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants a permission to a role in a tenant.
    /// </summary>
    Task GrantPermissionToRoleAsync(
        Guid tenantId,
        string roleName,
        string permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a permission from a role in a tenant.
    /// </summary>
    Task RevokePermissionFromRoleAsync(
        Guid tenantId,
        string roleName,
        string permission,
        CancellationToken cancellationToken = default);
}

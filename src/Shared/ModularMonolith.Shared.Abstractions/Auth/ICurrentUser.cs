namespace ModularMonolith.Shared.Abstractions.Auth;

/// <summary>
/// Interface for accessing the current authenticated user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the email of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the tenant identifier the user belongs to (null for staff users).
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the roles assigned to the current user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets the permissions assigned to the current user.
    /// </summary>
    IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets a value indicating whether the user is staff (system-wide access).
    /// </summary>
    bool IsStaff { get; }

    /// <summary>
    /// Gets a value indicating whether the user is a tenant owner.
    /// </summary>
    bool IsTenantOwner { get; }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role; otherwise, false.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check (e.g., "customers.read").</param>
    /// <returns>True if the user has the permission; otherwise, false.</returns>
    bool HasPermission(string permission);

    /// <summary>
    /// Checks if the current user has all specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the user has all permissions; otherwise, false.</returns>
    bool HasAllPermissions(params string[] permissions);

    /// <summary>
    /// Checks if the current user has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the user has any of the permissions; otherwise, false.</returns>
    bool HasAnyPermission(params string[] permissions);

    /// <summary>
    /// Gets a claim value from the current user's claims.
    /// </summary>
    /// <param name="claimType">The type of the claim.</param>
    /// <returns>The claim value if found; otherwise, null.</returns>
    string? GetClaim(string claimType);
}

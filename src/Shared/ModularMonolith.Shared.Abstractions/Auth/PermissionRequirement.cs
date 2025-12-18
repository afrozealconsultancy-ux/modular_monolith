using Microsoft.AspNetCore.Authorization;

namespace ModularMonolith.Shared.Abstractions.Auth;

/// <summary>
/// Authorization requirement for permission-based access control.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public PermissionRequirement(string[] permissions, bool requireAll = true)
    {
        Permissions = permissions;
        RequireAll = requireAll;
    }

    /// <summary>
    /// Single permission required.
    /// </summary>
    public string? Permission { get; }

    /// <summary>
    /// Multiple permissions.
    /// </summary>
    public string[]? Permissions { get; }

    /// <summary>
    /// If true, user must have all permissions. If false, user must have any permission.
    /// </summary>
    public bool RequireAll { get; }
}

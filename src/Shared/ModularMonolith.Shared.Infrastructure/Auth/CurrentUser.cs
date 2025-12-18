using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularMonolith.Shared.Abstractions.Auth;

namespace ModularMonolith.Shared.Infrastructure.Auth;

/// <summary>
/// Implementation of ICurrentUser that reads from HttpContext.User.
/// </summary>
internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<ClaimsPrincipal?> _user;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _user = new Lazy<ClaimsPrincipal?>(() => _httpContextAccessor.HttpContext?.User);
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _user.Value?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _user.Value?.FindFirst("sub")?.Value;

            return string.IsNullOrEmpty(userIdClaim) ? null : Guid.Parse(userIdClaim);
        }
    }

    public string? UserName => _user.Value?.FindFirst(ClaimTypes.Name)?.Value
        ?? _user.Value?.FindFirst("preferred_username")?.Value;

    public string? Email => _user.Value?.FindFirst(ClaimTypes.Email)?.Value
        ?? _user.Value?.FindFirst("email")?.Value;

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _user.Value?.FindFirst("tenant_id")?.Value;
            return string.IsNullOrEmpty(tenantIdClaim) ? null : Guid.Parse(tenantIdClaim);
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var roles = _user.Value?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();

            // Also check Keycloak realm_access.roles format
            var realmRoles = _user.Value?.FindAll("realm_roles")
                .Select(c => c.Value);

            if (realmRoles != null)
            {
                roles.AddRange(realmRoles);
            }

            return roles.Distinct().ToList();
        }
    }

    public IReadOnlyList<string> Permissions
    {
        get
        {
            return _user.Value?.FindAll("permission")
                .Select(c => c.Value)
                .Distinct()
                .ToList() ?? new List<string>();
        }
    }

    public bool IsAuthenticated => _user.Value?.Identity?.IsAuthenticated ?? false;

    public bool IsStaff
    {
        get
        {
            var realm = _user.Value?.FindFirst("realm")?.Value;
            return realm == "staff";
        }
    }

    public bool IsTenantOwner => Roles.Contains(Abstractions.Auth.Roles.Tenant.Owner);

    public bool IsInRole(string role) => Roles.Contains(role);

    public bool HasPermission(string permission) => Permissions.Contains(permission);

    public bool HasAllPermissions(params string[] permissions) => permissions.All(HasPermission);

    public bool HasAnyPermission(params string[] permissions) => permissions.Any(HasPermission);

    public string? GetClaim(string claimType) => _user.Value?.FindFirst(claimType)?.Value;
}

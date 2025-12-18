using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Shared.Infrastructure.Tenancy;

/// <summary>
/// Implementation of ITenantContext that resolves tenant from current user.
/// </summary>
internal sealed class TenantContext : ITenantContext
{
    private readonly ICurrentUser _currentUser;

    public TenantContext(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Guid? TenantId => _currentUser.TenantId;

    public string? TenantName => _currentUser.GetClaim("tenant_name");

    public bool IsStaff => _currentUser.IsStaff;
}

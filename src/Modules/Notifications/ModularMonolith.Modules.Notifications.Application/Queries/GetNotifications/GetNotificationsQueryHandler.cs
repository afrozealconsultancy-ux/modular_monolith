using MediatR;
using ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Notifications.Application.Queries.GetNotifications;

internal sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly INotificationsQueryService _queryService;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public GetNotificationsQueryHandler(
        INotificationsQueryService queryService,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _queryService = queryService;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Notifications.View))
        {
            return Result.Failure<PagedResult<NotificationDto>>(Error.Forbidden(
                "Notifications.GetNotifications.Forbidden",
                "You don't have permission to view notifications"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<PagedResult<NotificationDto>>(Error.Unauthorized(
                "Notifications.GetNotifications.NoTenant",
                "Tenant context is required"));
        }

        var result = await _queryService.GetNotificationsAsync(
            request.Page,
            request.PageSize,
            request.Type,
            request.Status,
            cancellationToken);

        return Result.Success(result);
    }
}

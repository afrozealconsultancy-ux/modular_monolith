using MediatR;
using ModularMonolith.Modules.Notifications.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;

internal sealed class GetNotificationQueryHandler : IRequestHandler<GetNotificationQuery, Result<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public GetNotificationQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<Result<NotificationDto>> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
    {
        // Check permission
        if (!_currentUser.HasPermission(Permissions.Notifications.View))
        {
            return Result.Failure<NotificationDto>(Error.Forbidden(
                "Notifications.GetNotification.Forbidden",
                "You don't have permission to view notifications"));
        }

        // Ensure tenant context
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<NotificationDto>(Error.Unauthorized(
                "Notifications.GetNotification.NoTenant",
                "Tenant context is required"));
        }

        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification == null)
        {
            return Result.Failure<NotificationDto>(Error.NotFound(
                "Notifications.GetNotification.NotFound",
                $"Notification with ID '{request.NotificationId}' was not found"));
        }

        var dto = new NotificationDto(
            notification.Id,
            notification.Type.ToString(),
            notification.Recipient,
            notification.Subject,
            notification.Content,
            notification.Status.ToString(),
            notification.CreatedAt,
            notification.SentAt,
            notification.FailureReason,
            notification.RetryCount);

        return Result.Success(dto);
    }
}

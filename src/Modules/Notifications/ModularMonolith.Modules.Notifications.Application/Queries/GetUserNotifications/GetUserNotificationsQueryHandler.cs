using MediatR;
using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Modules.Notifications.Domain.Repositories;
using ModularMonolith.Shared.Abstractions.Auth;
using ModularMonolith.Shared.Abstractions.CQRS;
using ModularMonolith.Shared.Abstractions.Kernel;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Notifications.Application.Queries.GetUserNotifications;

internal sealed class GetUserNotificationsQueryHandler
    : IRequestHandler<GetUserNotificationsQuery, Result<UserNotificationsResponse>>
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public GetUserNotificationsQueryHandler(
        INotificationRecipientRepository recipientRepository,
        ICurrentUser currentUser,
        ITenantContext tenantContext)
    {
        _recipientRepository = recipientRepository;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<Result<UserNotificationsResponse>> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant)
        {
            return Result.Failure<UserNotificationsResponse>(Error.Unauthorized(
                "Notifications.GetUser.NoTenant",
                "Tenant context is required"));
        }

        try
        {
            var recipients = await _recipientRepository.GetUnreadForUserAsync(
                _currentUser.UserId,
                _tenantContext.TenantId!.Value,
                cancellationToken);

            var notifications = recipients
                .Select(r => new UserNotificationDto(
                    r.NotificationId,
                    r.Id,
                    r.DeliveryChannel.ToString(),
                    r.Notification.Subject,
                    r.Notification.Content,
                    r.Notification.CreatedAt,
                    r.IsRead,
                    r.ReadAt,
                    r.DeliveryStatus == NotificationStatus.Sent,
                    r.SentAt))
                .ToList();

            var unreadCount = notifications.Count(n => !n.IsRead);

            var response = new UserNotificationsResponse(
                notifications,
                notifications.Count,
                unreadCount);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserNotificationsResponse>(Error.Failure(
                "Notifications.GetUser.Failed",
                $"Failed to retrieve notifications: {ex.Message}"));
        }
    }
}

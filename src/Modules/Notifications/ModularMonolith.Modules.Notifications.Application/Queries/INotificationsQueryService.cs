using ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;
using ModularMonolith.Modules.Notifications.Application.Queries.GetNotifications;

namespace ModularMonolith.Modules.Notifications.Application.Queries;

public interface INotificationsQueryService
{
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        int page,
        int pageSize,
        string? type,
        string? status,
        CancellationToken cancellationToken = default);
}

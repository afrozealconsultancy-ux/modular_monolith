using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Notifications.Application.Queries.GetUserNotifications;

public sealed record GetUserNotificationsQuery(
    bool OnlyUnread = false,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<Result<UserNotificationsResponse>>;

public sealed record UserNotificationsResponse(
    List<UserNotificationDto> Notifications,
    int TotalCount,
    int UnreadCount);

public sealed record UserNotificationDto(
    Guid NotificationId,
    Guid RecipientId,
    string Type,
    string? Subject,
    string Content,
    DateTime CreatedAt,
    bool IsRead,
    DateTime? ReadAt,
    bool WasSent,
    DateTime? SentAt);

using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;

public sealed record GetNotificationQuery(Guid NotificationId) : IQuery<Result<NotificationDto>>;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Recipient,
    string? Subject,
    string Content,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt,
    string? FailureReason,
    int RetryCount);

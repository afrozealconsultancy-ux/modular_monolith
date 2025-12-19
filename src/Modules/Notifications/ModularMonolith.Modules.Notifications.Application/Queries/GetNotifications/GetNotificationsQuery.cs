using ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;
using ModularMonolith.Shared.Abstractions.CQRS;

namespace ModularMonolith.Modules.Notifications.Application.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Type = null,
    string? Status = null) : IQuery<Result<PagedResult<NotificationDto>>>;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Notifications.Application.Queries;
using ModularMonolith.Modules.Notifications.Application.Queries.GetNotification;
using ModularMonolith.Modules.Notifications.Application.Queries.GetNotifications;
using ModularMonolith.Modules.Notifications.Domain.Enums;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Queries;

internal sealed class NotificationsQueryService : INotificationsQueryService
{
    private readonly NotificationsDbContext _dbContext;

    public NotificationsQueryService(NotificationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        int page,
        int pageSize,
        string? type,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<NotificationType>(type, true, out var notificationType))
        {
            query = query.Where(n => n.Type == notificationType);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<NotificationStatus>(status, true, out var notificationStatus))
        {
            query = query.Where(n => n.Status == notificationStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                n.Recipient,
                n.Subject,
                n.Content,
                n.Status.ToString(),
                n.CreatedAt,
                n.SentAt,
                n.FailureReason,
                n.RetryCount))
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>(notifications, page, pageSize, totalCount);
    }
}

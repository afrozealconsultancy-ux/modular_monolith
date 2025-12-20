using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Notifications.Domain.Entities;
using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Modules.Notifications.Domain.Repositories;
using ModularMonolith.Shared.Infrastructure.Persistence;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository : Repository<Notification, Guid>, INotificationRepository
{
    public NotificationRepository(NotificationsDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetUnprocessedNotificationsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .Where(n => !n.IsProcessed)
            .OrderBy(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByScopeAsync(
        NotificationScope scope,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .Where(n => n.Scope == scope)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByCreatorAsync(
        Guid createdByUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .Where(n => n.CreatedByUserId == createdByUserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetWithRecipientsAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Notifications
            .Include(n => n.Recipients)
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);
    }

    private new NotificationsDbContext DbContext => (NotificationsDbContext)base.DbContext;
}

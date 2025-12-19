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

    public async Task<IReadOnlyList<Notification>> GetPendingNotificationsAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Notification>()
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

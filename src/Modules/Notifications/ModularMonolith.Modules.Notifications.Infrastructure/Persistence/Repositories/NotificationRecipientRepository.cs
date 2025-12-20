using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.Notifications.Domain.Entities;
using ModularMonolith.Modules.Notifications.Domain.Repositories;

namespace ModularMonolith.Modules.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRecipientRepository : INotificationRecipientRepository
{
    private readonly NotificationsDbContext _dbContext;

    public NotificationRecipientRepository(NotificationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationRecipient?> GetByIdAsync(
        Guid recipientId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationRecipients
            .Include(r => r.Notification)
            .FirstOrDefaultAsync(r => r.Id == recipientId, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationRecipient>> GetUnreadForUserAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationRecipients
            .Include(r => r.Notification)
            .Where(r => r.UserId == userId && r.TenantId == tenantId && !r.IsRead)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationRecipient>> GetPendingRecipientsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationRecipients
            .Where(r => r.DeliveryStatus == Domain.Enums.NotificationStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAllAsReadForUserAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationRecipients
            .Where(r => r.UserId == userId && r.TenantId == tenantId && !r.IsRead)
            .ForEachAsync(r => r.MarkAsRead(), cancellationToken);
    }
}

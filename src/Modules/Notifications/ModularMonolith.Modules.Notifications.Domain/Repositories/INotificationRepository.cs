using ModularMonolith.Modules.Notifications.Domain.Entities;
using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.Persistence;

namespace ModularMonolith.Modules.Notifications.Domain.Repositories;

/// <summary>
/// Repository for notifications.
/// </summary>
public interface INotificationRepository : IRepository<Notification, Guid>
{
    /// <summary>
    /// Gets unprocessed notifications that need recipient resolution.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetUnprocessedNotificationsAsync(
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications by scope.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetByScopeAsync(
        NotificationScope scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications created by a specific user.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetByCreatorAsync(
        Guid createdByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a notification with all its recipients.
    /// </summary>
    Task<Notification?> GetWithRecipientsAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);
}

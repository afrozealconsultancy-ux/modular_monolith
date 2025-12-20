using ModularMonolith.Modules.Notifications.Domain.Entities;

namespace ModularMonolith.Modules.Notifications.Domain.Repositories;

/// <summary>
/// Repository for notification recipients.
/// Note: NotificationRecipient is an Entity, not an AggregateRoot, so it doesn't use IRepository.
/// </summary>
public interface INotificationRecipientRepository
{
    /// <summary>
    /// Gets a recipient by ID.
    /// </summary>
    Task<NotificationRecipient?> GetByIdAsync(
        Guid recipientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unread notification recipients for a specific user.
    /// </summary>
    Task<IReadOnlyList<NotificationRecipient>> GetUnreadForUserAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending (unsent) recipients up to a limit for batch processing.
    /// </summary>
    Task<IReadOnlyList<NotificationRecipient>> GetPendingRecipientsAsync(
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications as read for a specific user.
    /// </summary>
    Task MarkAllAsReadForUserAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

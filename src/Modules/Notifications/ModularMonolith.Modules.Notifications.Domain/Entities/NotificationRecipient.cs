using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Entities;

/// <summary>
/// Represents an individual recipient of a notification.
/// Tracks delivery status and read status per user.
/// </summary>
public sealed class NotificationRecipient : Entity
{
    private NotificationRecipient(
        Guid id,
        Guid notificationId,
        Guid userId,
        Guid tenantId,
        NotificationType deliveryChannel,
        string recipientAddress)
        : base(id)
    {
        NotificationId = notificationId;
        UserId = userId;
        TenantId = tenantId;
        DeliveryChannel = deliveryChannel;
        RecipientAddress = recipientAddress;
        DeliveryStatus = NotificationStatus.Pending;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public NotificationType DeliveryChannel { get; private set; }
    public string RecipientAddress { get; private set; } // Email, phone number, etc.
    public NotificationStatus DeliveryStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Navigation property
    public Notification Notification { get; private set; } = null!;

    public static NotificationRecipient Create(
        Guid notificationId,
        Guid userId,
        Guid tenantId,
        NotificationType deliveryChannel,
        string recipientAddress)
    {
        if (string.IsNullOrWhiteSpace(recipientAddress))
            throw new ArgumentException("Recipient address cannot be empty", nameof(recipientAddress));

        return new NotificationRecipient(
            Guid.NewGuid(),
            notificationId,
            userId,
            tenantId,
            deliveryChannel,
            recipientAddress);
    }

    public void MarkAsSending()
    {
        if (DeliveryStatus != NotificationStatus.Pending)
            throw new InvalidOperationException($"Cannot mark as sending when status is {DeliveryStatus}");

        DeliveryStatus = NotificationStatus.Sending;
    }

    public void MarkAsSent()
    {
        if (DeliveryStatus != NotificationStatus.Sending)
            throw new InvalidOperationException($"Cannot mark as sent when status is {DeliveryStatus}");

        DeliveryStatus = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(failureReason));

        DeliveryStatus = NotificationStatus.Failed;
        FailureReason = failureReason;
        RetryCount++;
    }

    public void MarkAsRead()
    {
        if (DeliveryStatus != NotificationStatus.Sent)
            throw new InvalidOperationException("Can only mark sent notifications as read");

        if (IsRead)
            return; // Already read

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void Retry()
    {
        if (DeliveryStatus != NotificationStatus.Failed)
            throw new InvalidOperationException("Can only retry failed deliveries");

        DeliveryStatus = NotificationStatus.Pending;
        FailureReason = null;
    }
}

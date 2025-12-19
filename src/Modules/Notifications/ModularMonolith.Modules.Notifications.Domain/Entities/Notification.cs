using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Modules.Notifications.Domain.Events;
using ModularMonolith.Shared.Abstractions.Tenancy;

namespace ModularMonolith.Modules.Notifications.Domain.Entities;

public sealed class Notification : TenantAggregateRoot
{
    private Notification(
        Guid id,
        Guid tenantId,
        NotificationType type,
        string recipient,
        string? subject,
        string content)
        : base(id, tenantId)
    {
        Type = type;
        Recipient = recipient;
        Subject = subject;
        Content = content;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public NotificationType Type { get; private set; }
    public string Recipient { get; private set; }
    public string? Subject { get; private set; }
    public string Content { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }

    public static Notification Create(
        Guid tenantId,
        NotificationType type,
        string recipient,
        string? subject,
        string content)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty", nameof(recipient));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        // Validate subject is required for email
        if (type == NotificationType.Email && string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required for email notifications", nameof(subject));

        var notification = new Notification(
            Guid.NewGuid(),
            tenantId,
            type,
            recipient,
            subject,
            content);

        return notification;
    }

    public void MarkAsSending()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException($"Cannot mark notification as sending when status is {Status}");

        Status = NotificationStatus.Sending;
    }

    public void MarkAsSent()
    {
        if (Status != NotificationStatus.Sending)
            throw new InvalidOperationException($"Cannot mark notification as sent when status is {Status}");

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationSentDomainEvent(
            Id,
            TenantId,
            Type,
            Recipient));
    }

    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(failureReason));

        Status = NotificationStatus.Failed;
        FailureReason = failureReason;
        RetryCount++;

        RaiseDomainEvent(new NotificationFailedDomainEvent(
            Id,
            TenantId,
            Type,
            Recipient,
            FailureReason));
    }

    public void Retry()
    {
        if (Status != NotificationStatus.Failed)
            throw new InvalidOperationException("Can only retry failed notifications");

        Status = NotificationStatus.Pending;
        FailureReason = null;
    }
}

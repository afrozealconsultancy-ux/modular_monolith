using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Modules.Notifications.Domain.Events;
using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Entities;

/// <summary>
/// Notification aggregate root supporting cross-tenant broadcasts.
/// Can be scoped to personal, tenant-wide, or cross-tenant delivery.
/// </summary>
public sealed class Notification : AggregateRoot
{
    private readonly List<NotificationRecipient> _recipients = new();
    private readonly List<Guid> _targetTenantIds = new();

    private Notification(
        Guid id,
        NotificationScope scope,
        NotificationType type,
        string? subject,
        string content,
        Guid? createdByTenantId,
        Guid createdByUserId)
        : base(id)
    {
        Scope = scope;
        Type = type;
        Subject = subject;
        Content = content;
        CreatedByTenantId = createdByTenantId;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public NotificationScope Scope { get; private set; }
    public NotificationType Type { get; private set; }
    public string? Subject { get; private set; }
    public string Content { get; private set; }
    public Guid? CreatedByTenantId { get; private set; } // Null for system notifications
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    public bool IsProcessed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Target tenants for SelectedTenants scope
    public IReadOnlyList<Guid> TargetTenantIds => _targetTenantIds.AsReadOnly();

    // Individual recipients with delivery tracking
    public IReadOnlyList<NotificationRecipient> Recipients => _recipients.AsReadOnly();

    /// <summary>
    /// Creates a personal notification to specific users.
    /// </summary>
    public static Notification CreatePersonal(
        NotificationType type,
        string? subject,
        string content,
        Guid createdByTenantId,
        Guid createdByUserId)
    {
        ValidateContent(type, subject, content);

        return new Notification(
            Guid.NewGuid(),
            NotificationScope.Personal,
            type,
            subject,
            content,
            createdByTenantId,
            createdByUserId);
    }

    /// <summary>
    /// Creates a tenant-wide broadcast notification.
    /// </summary>
    public static Notification CreateTenantBroadcast(
        NotificationType type,
        string? subject,
        string content,
        Guid tenantId,
        Guid createdByUserId)
    {
        ValidateContent(type, subject, content);

        return new Notification(
            Guid.NewGuid(),
            NotificationScope.Tenant,
            type,
            subject,
            content,
            tenantId,
            createdByUserId);
    }

    /// <summary>
    /// Creates a notification for selected tenants (admin only).
    /// </summary>
    public static Notification CreateForSelectedTenants(
        NotificationType type,
        string? subject,
        string content,
        IEnumerable<Guid> targetTenantIds,
        Guid createdByUserId)
    {
        ValidateContent(type, subject, content);

        var notification = new Notification(
            Guid.NewGuid(),
            NotificationScope.SelectedTenants,
            type,
            subject,
            content,
            null, // Cross-tenant, no single tenant owner
            createdByUserId);

        var tenantIdsList = targetTenantIds.ToList();
        if (tenantIdsList.Count == 0)
            throw new ArgumentException("Must specify at least one target tenant", nameof(targetTenantIds));

        notification._targetTenantIds.AddRange(tenantIdsList);

        return notification;
    }

    /// <summary>
    /// Creates an all-tenants broadcast notification (admin only).
    /// </summary>
    public static Notification CreateAllTenantsBroadcast(
        NotificationType type,
        string? subject,
        string content,
        Guid createdByUserId)
    {
        ValidateContent(type, subject, content);

        return new Notification(
            Guid.NewGuid(),
            NotificationScope.AllTenants,
            type,
            subject,
            content,
            null, // Cross-tenant, no single tenant owner
            createdByUserId);
    }

    /// <summary>
    /// Adds a recipient to this notification.
    /// </summary>
    public void AddRecipient(Guid userId, Guid tenantId, string recipientAddress)
    {
        if (IsProcessed)
            throw new InvalidOperationException("Cannot add recipients to a processed notification");

        if (string.IsNullOrWhiteSpace(recipientAddress))
            throw new ArgumentException("Recipient address cannot be empty", nameof(recipientAddress));

        // Check for duplicates
        if (_recipients.Any(r => r.UserId == userId && r.TenantId == tenantId))
            return; // Already added

        var recipient = NotificationRecipient.Create(
            Id,
            userId,
            tenantId,
            Type,
            recipientAddress);

        _recipients.Add(recipient);
    }

    /// <summary>
    /// Adds multiple recipients at once.
    /// </summary>
    public void AddRecipients(IEnumerable<(Guid UserId, Guid TenantId, string Address)> recipients)
    {
        foreach (var (userId, tenantId, address) in recipients)
        {
            AddRecipient(userId, tenantId, address);
        }
    }

    /// <summary>
    /// Marks the notification as processed (recipients have been resolved).
    /// </summary>
    public void MarkAsProcessed()
    {
        if (IsProcessed)
            throw new InvalidOperationException("Notification is already processed");

        if (_recipients.Count == 0)
            throw new InvalidOperationException("Cannot process notification with no recipients");

        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationCreatedDomainEvent(
            Id,
            Scope,
            Type,
            _recipients.Count));
    }

    /// <summary>
    /// Schedules the notification for future delivery.
    /// </summary>
    public void ScheduleFor(DateTime scheduledTime)
    {
        if (scheduledTime <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled time must be in the future", nameof(scheduledTime));

        ScheduledFor = scheduledTime;
    }

    private static void ValidateContent(NotificationType type, string? subject, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        // Validate subject is required for email
        if (type == NotificationType.Email && string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required for email notifications", nameof(subject));
    }
}

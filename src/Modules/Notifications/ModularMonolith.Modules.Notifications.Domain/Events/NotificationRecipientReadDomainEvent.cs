using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Events;

/// <summary>
/// Raised when a user marks a notification as read.
/// </summary>
public sealed record NotificationRecipientReadDomainEvent(
    Guid NotificationId,
    Guid RecipientId,
    Guid UserId,
    Guid TenantId,
    DateTime ReadAt) : DomainEvent;

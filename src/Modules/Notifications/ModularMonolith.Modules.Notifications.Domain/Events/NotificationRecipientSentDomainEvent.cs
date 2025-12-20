using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Events;

/// <summary>
/// Raised when a notification is successfully sent to a specific recipient.
/// </summary>
public sealed record NotificationRecipientSentDomainEvent(
    Guid NotificationId,
    Guid RecipientId,
    Guid UserId,
    Guid TenantId) : DomainEvent;

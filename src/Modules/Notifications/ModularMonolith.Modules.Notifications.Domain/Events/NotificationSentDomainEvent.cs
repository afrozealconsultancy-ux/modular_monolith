using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Events;

public sealed record NotificationSentDomainEvent(
    Guid NotificationId,
    Guid TenantId,
    NotificationType Type,
    string Recipient) : DomainEvent;

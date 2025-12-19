using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Events;

public sealed record NotificationFailedDomainEvent(
    Guid NotificationId,
    Guid TenantId,
    NotificationType Type,
    string Recipient,
    string FailureReason) : DomainEvent;

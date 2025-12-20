using ModularMonolith.Modules.Notifications.Domain.Enums;
using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Notifications.Domain.Events;

public sealed record NotificationCreatedDomainEvent(
    Guid NotificationId,
    NotificationScope Scope,
    NotificationType Type,
    int RecipientCount) : DomainEvent;

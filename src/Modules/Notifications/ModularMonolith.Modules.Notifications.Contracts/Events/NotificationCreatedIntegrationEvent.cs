using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Notifications.Contracts.Events;

/// <summary>
/// Published when a notification is created and ready for delivery.
/// </summary>
public sealed record NotificationCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid NotificationId { get; init; }
    public required string Scope { get; init; }
    public required string Type { get; init; }
    public required int RecipientCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ScheduledFor { get; init; }
}

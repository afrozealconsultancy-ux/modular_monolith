using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Notifications.Contracts.Events;

public sealed record NotificationSentIntegrationEvent : IntegrationEvent
{
    public required Guid NotificationId { get; init; }
    public required Guid TenantId { get; init; }
    public required string Type { get; init; }
    public required string Recipient { get; init; }
    public required DateTime SentAt { get; init; }
}

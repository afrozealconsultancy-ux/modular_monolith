using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Storage.Contracts.Events;

public sealed record FileDeletedIntegrationEvent : IntegrationEvent
{
    public required Guid FileId { get; init; }
    public required Guid TenantId { get; init; }
    public required string FileName { get; init; }
}

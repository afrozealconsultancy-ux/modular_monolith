using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Storage.Contracts.Events;

public sealed record FileUploadedIntegrationEvent : IntegrationEvent
{
    public required Guid FileId { get; init; }
    public required Guid TenantId { get; init; }
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required string MimeType { get; init; }
    public required Guid UploadedBy { get; init; }
    public required DateTime UploadedAt { get; init; }
}

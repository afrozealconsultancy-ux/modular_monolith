using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Storage.Domain.Events;

public sealed record FileUploadedDomainEvent(
    Guid FileId,
    Guid TenantId,
    string FileName,
    long FileSize,
    string MimeType,
    Guid UploadedBy) : DomainEvent;

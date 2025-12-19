using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Storage.Domain.Events;

public sealed record FileDeletedDomainEvent(
    Guid FileId,
    Guid TenantId,
    string FileName) : DomainEvent;

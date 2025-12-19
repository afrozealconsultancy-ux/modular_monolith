using ModularMonolith.Modules.Storage.Domain.Enums;
using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Storage.Domain.Events;

public sealed record FileScanCompletedDomainEvent(
    Guid FileId,
    Guid TenantId,
    FileScanStatus ScanStatus,
    string? ScanResult) : DomainEvent;

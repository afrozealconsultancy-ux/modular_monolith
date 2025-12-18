using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Customers.Domain.Events;

/// <summary>
/// Domain event raised when a customer is deleted.
/// </summary>
public sealed record CustomerDeletedDomainEvent(
    Guid CustomerId,
    Guid TenantId) : DomainEvent;

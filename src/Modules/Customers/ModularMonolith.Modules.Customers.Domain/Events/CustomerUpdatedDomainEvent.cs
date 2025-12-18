using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Customers.Domain.Events;

/// <summary>
/// Domain event raised when a customer is updated.
/// </summary>
public sealed record CustomerUpdatedDomainEvent(
    Guid CustomerId,
    Guid TenantId,
    string Name,
    string Email) : DomainEvent;

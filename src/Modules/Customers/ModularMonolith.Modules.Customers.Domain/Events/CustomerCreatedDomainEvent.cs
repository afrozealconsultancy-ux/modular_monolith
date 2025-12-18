using ModularMonolith.Shared.Abstractions.Domain;

namespace ModularMonolith.Modules.Customers.Domain.Events;

/// <summary>
/// Domain event raised when a customer is created.
/// Internal to Customers module - handled in same transaction.
/// </summary>
public sealed record CustomerCreatedDomainEvent(
    Guid CustomerId,
    Guid TenantId,
    string Name,
    string Email) : DomainEvent;

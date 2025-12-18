using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Modules.Customers.Contracts.Events;

/// <summary>
/// Integration event published when a customer is created.
/// OTHER modules can subscribe to this event.
/// </summary>
public sealed record CustomerCreatedIntegrationEvent : IntegrationEvent
{
    public required Guid CustomerId { get; init; }
    public required Guid TenantId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
}

namespace ModularMonolith.Shared.Abstractions.Messaging;

/// <summary>
/// Base class for integration events.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    protected IntegrationEvent(Guid eventId, DateTime occurredOnUtc, string? correlationId = null)
    {
        EventId = eventId;
        OccurredOnUtc = occurredOnUtc;
        CorrelationId = correlationId;
    }

    public Guid EventId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public string? CorrelationId { get; init; }
}

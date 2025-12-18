namespace ModularMonolith.Shared.Abstractions.Messaging;

/// <summary>
/// Base interface for integration events.
/// Integration events are used for cross-module communication via the event bus (RabbitMQ).
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the date and time when the event occurred (UTC).
    /// </summary>
    DateTime OccurredOnUtc { get; }

    /// <summary>
    /// Gets the correlation ID for tracing related events.
    /// </summary>
    string? CorrelationId { get; }
}

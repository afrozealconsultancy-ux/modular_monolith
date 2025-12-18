namespace ModularMonolith.Shared.Abstractions.Messaging;

/// <summary>
/// Interface for publishing integration events to the event bus.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the event bus.
    /// </summary>
    /// <typeparam name="TEvent">The type of the integration event.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;

    /// <summary>
    /// Publishes multiple integration events to the event bus.
    /// </summary>
    /// <param name="events">The events to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync(IEnumerable<IIntegrationEvent> events, CancellationToken cancellationToken = default);
}

using MassTransit;
using ModularMonolith.Shared.Abstractions.Messaging;

namespace ModularMonolith.Shared.Infrastructure.EventBus;

/// <summary>
/// MassTransit implementation of IEventBus for publishing integration events.
/// </summary>
internal sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IIntegrationEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            await _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}

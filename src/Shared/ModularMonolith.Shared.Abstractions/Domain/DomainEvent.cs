namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid EventId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}

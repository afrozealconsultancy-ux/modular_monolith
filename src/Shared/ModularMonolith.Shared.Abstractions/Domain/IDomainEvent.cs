namespace ModularMonolith.Shared.Abstractions.Domain;

/// <summary>
/// Base interface for domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// They are handled within the same module/bounded context.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the date and time when the event occurred (UTC).
    /// </summary>
    DateTime OccurredOnUtc { get; }
}

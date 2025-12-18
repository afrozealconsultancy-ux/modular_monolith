namespace ModularMonolith.Shared.Abstractions.Time;

/// <summary>
/// Interface for providing current date and time.
/// Useful for testing and controlling time in the application.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    DateTime Now { get; }
}

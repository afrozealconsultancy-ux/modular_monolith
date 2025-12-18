using ModularMonolith.Shared.Abstractions.Time;

namespace ModularMonolith.Shared.Infrastructure.Time;

/// <summary>
/// System clock implementation of IDateTimeProvider.
/// </summary>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}

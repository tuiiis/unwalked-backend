using Unwalked.Application.Abstractions;

namespace Unwalked.UnitTests.Fakes;

public sealed class FixedClock(DateTimeOffset now) : IClock
{
    public static readonly DateTimeOffset Default = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);

    public FixedClock()
        : this(Default)
    {
    }

    public DateTimeOffset UtcNow { get; } = now;
}

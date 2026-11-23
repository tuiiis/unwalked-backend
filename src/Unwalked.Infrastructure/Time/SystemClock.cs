using Unwalked.Application.Abstractions;

namespace Unwalked.Infrastructure.Time;

/// <summary>
/// Clock backed by the system time.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

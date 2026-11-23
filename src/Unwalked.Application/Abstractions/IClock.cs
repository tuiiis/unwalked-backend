namespace Unwalked.Application.Abstractions;

/// <summary>
/// Source of the current time, replaceable in tests.
/// </summary>
public interface IClock
{
    /// <summary>
    /// The current time in UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}

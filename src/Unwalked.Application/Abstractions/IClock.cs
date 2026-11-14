namespace Unwalked.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

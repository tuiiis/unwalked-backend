namespace Unwalked.Application.Errors;

/// <summary>
/// The request is valid but a limit was reached. Maps to 429 with a Retry-After header.
/// </summary>
public sealed class TooManyRequestsException(string message, TimeSpan retryAfter) : Exception(message)
{
    /// <summary>
    /// How long the client should wait before trying again.
    /// </summary>
    public TimeSpan RetryAfter { get; } = retryAfter;
}

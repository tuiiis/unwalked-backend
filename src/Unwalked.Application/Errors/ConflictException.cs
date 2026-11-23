namespace Unwalked.Application.Errors;

/// <summary>
/// Thrown when a request conflicts with existing data. Mapped to 409.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);

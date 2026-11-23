namespace Unwalked.Application.Errors;

/// <summary>
/// Thrown when a requested resource does not exist or is not visible to the caller. Mapped to 404.
/// </summary>
public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.");

namespace Unwalked.Application.Errors;

/// <summary>
/// Thrown when sign-in credentials are wrong. Mapped to 401.
/// </summary>
public sealed class AuthenticationFailedException() : Exception("Email or password is incorrect.");

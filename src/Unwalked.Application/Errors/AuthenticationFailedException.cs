namespace Unwalked.Application.Errors;

/// <summary>
/// Thrown when sign-in credentials are wrong or a token belongs to a deleted account. Mapped to 401.
/// </summary>
public sealed class AuthenticationFailedException(string message = "Email or password is incorrect.") : Exception(message);

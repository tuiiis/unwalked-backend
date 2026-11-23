namespace Unwalked.Application.Auth;

/// <summary>
/// A signed access token and the moment it expires.
/// </summary>
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

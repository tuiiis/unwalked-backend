namespace Unwalked.Application.Auth;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

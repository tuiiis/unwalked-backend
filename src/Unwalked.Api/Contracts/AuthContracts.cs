using System.ComponentModel.DataAnnotations;

namespace Unwalked.Api.Contracts;

public sealed record RegisterRequest([Required] string Email, [Required] string Password);

public sealed record RegisterResponse(Guid UserId);

public sealed record LoginRequest([Required] string Email, [Required] string Password);

public sealed record LoginResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt);

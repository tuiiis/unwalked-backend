using System.ComponentModel.DataAnnotations;

namespace Unwalked.Api.Contracts;

/// <summary>
/// Body of a registration request.
/// </summary>
public sealed record RegisterRequest([Required] string Email, [Required] string Password);

/// <summary>
/// Identifier of the account that was just created.
/// </summary>
public sealed record RegisterResponse(Guid UserId);

/// <summary>
/// Body of a sign-in request.
/// </summary>
public sealed record LoginRequest([Required] string Email, [Required] string Password);

/// <summary>
/// Access token returned after a successful sign-in, with its type and expiry.
/// </summary>
public sealed record LoginResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt);

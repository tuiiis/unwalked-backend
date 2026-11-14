using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Unwalked.Application.Abstractions;
using Unwalked.Application.Auth;
using Unwalked.Domain.Users;

namespace Unwalked.Infrastructure.Security;

public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options, IClock clock) : IAccessTokenIssuer
{
    private readonly JsonWebTokenHandler handler = new();

    public AccessToken Issue(UserAccount user)
    {
        var settings = options.Value;
        var issuedAt = clock.UtcNow;
        var expiresAt = issuedAt.AddMinutes(settings.LifetimeMinutes);

        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = issuedAt.UtcDateTime,
            NotBefore = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())]),
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings), SecurityAlgorithms.HmacSha256),
        });

        return new AccessToken(token, expiresAt);
    }

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions settings) =>
        new(Encoding.UTF8.GetBytes(settings.SigningKey));
}

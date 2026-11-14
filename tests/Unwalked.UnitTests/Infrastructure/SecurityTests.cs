using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Unwalked.Domain.Users;
using Unwalked.Infrastructure.Security;
using Unwalked.UnitTests.Fakes;

namespace Unwalked.UnitTests.Infrastructure;

public class SecurityTests
{
    private static readonly JwtOptions Settings = new()
    {
        Issuer = "unwalked",
        Audience = "places-app",
        SigningKey = "unit-test-signing-key-that-is-long-enough",
        LifetimeMinutes = 30,
    };

    [Test]
    public void PasswordHasher_VerifiesOnlyTheOriginalPassword()
    {
        var hasher = new AspNetPasswordHasher();

        var hash = hasher.Hash("correct horse battery");

        Assert.Multiple(() =>
        {
            Assert.That(hash, Does.Not.Contain("correct horse"));
            Assert.That(hasher.Verify(hash, "correct horse battery"), Is.True);
            Assert.That(hasher.Verify(hash, "wrong horse battery"), Is.False);
        });
    }

    [Test]
    public async Task TokenIssuer_IssuesSignedTokenWithSubjectAndLifetime()
    {
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var issuer = new JwtAccessTokenIssuer(Options.Create(Settings), clock);
        var user = new UserAccount { Id = Guid.NewGuid(), Email = "walker@example.lt", PasswordHash = "x" };

        var token = issuer.Issue(user);

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, new TokenValidationParameters
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = JwtAccessTokenIssuer.CreateSigningKey(Settings),
        });

        Assert.Multiple(() =>
        {
            Assert.That(validation.IsValid, Is.True, validation.Exception?.Message);
            Assert.That(validation.Claims[JwtRegisteredClaimNames.Sub], Is.EqualTo(user.Id.ToString()));
            Assert.That(token.ExpiresAt, Is.EqualTo(clock.UtcNow.AddMinutes(Settings.LifetimeMinutes)));
        });
    }
}

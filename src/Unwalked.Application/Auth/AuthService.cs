using Unwalked.Application.Abstractions;
using Unwalked.Application.Errors;
using Unwalked.Domain.Users;

namespace Unwalked.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    IClock clock)
{
    public async Task<Guid> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailAddress.Normalize(email);
        PasswordPolicy.Validate(password);

        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(password),
            CreatedAt = clock.UtcNow,
        };

        if (!await users.TryAddAsync(user, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        return user.Id;
    }

    public async Task<AccessToken> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.Verify(user.PasswordHash, password))
        {
            throw new AuthenticationFailedException();
        }

        return tokenIssuer.Issue(user);
    }
}

using Unwalked.Application.Abstractions;
using Unwalked.Application.Errors;
using Unwalked.Domain.Users;

namespace Unwalked.Application.Auth;

/// <summary>
/// Registration, sign-in and account deletion use cases.
/// </summary>
public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer tokenIssuer,
    IClock clock)
{
    /// <summary>
    /// Validates the email and password, stores a new account and returns its id. Throws ConflictException when the email is taken.
    /// </summary>
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

    /// <summary>
    /// Checks the credentials and issues an access token. A wrong email and a wrong password fail the same way.
    /// </summary>
    public async Task<AccessToken> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.Verify(user.PasswordHash, password))
        {
            throw new AuthenticationFailedException();
        }

        return tokenIssuer.Issue(user);
    }

    /// <summary>
    /// Deletes the caller's account together with its shares. Repeating it after the account is gone does nothing.
    /// </summary>
    public Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken) =>
        users.DeleteAsync(userId, cancellationToken);
}

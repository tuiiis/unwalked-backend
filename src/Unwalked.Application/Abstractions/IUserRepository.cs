using Unwalked.Domain.Users;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Storage of user accounts.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Returns the account with this normalised email, or null.
    /// </summary>
    Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the account. Returns false when the email is already taken.
    /// </summary>
    Task<bool> TryAddAsync(UserAccount user, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the account and, through the database, its shares. Returns false when it no longer exists.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

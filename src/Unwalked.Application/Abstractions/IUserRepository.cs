using Unwalked.Domain.Users;

namespace Unwalked.Application.Abstractions;

public interface IUserRepository
{
    Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> TryAddAsync(UserAccount user, CancellationToken cancellationToken);
}

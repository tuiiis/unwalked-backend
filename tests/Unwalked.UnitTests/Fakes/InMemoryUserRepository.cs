using Unwalked.Application.Abstractions;
using Unwalked.Domain.Users;

namespace Unwalked.UnitTests.Fakes;

public sealed class InMemoryUserRepository : IUserRepository
{
    public Dictionary<string, UserAccount> Users { get; } = new(StringComparer.Ordinal);

    public Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(Users.GetValueOrDefault(normalizedEmail));

    public Task<bool> TryAddAsync(UserAccount user, CancellationToken cancellationToken) =>
        Task.FromResult(Users.TryAdd(user.Email, user));

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = Users.Values.SingleOrDefault(candidate => candidate.Id == id);
        return Task.FromResult(user is not null && Users.Remove(user.Email));
    }
}

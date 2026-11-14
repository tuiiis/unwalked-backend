using Unwalked.Application.Abstractions;
using Unwalked.Application.Auth;
using Unwalked.Domain.Users;

namespace Unwalked.UnitTests.Fakes;

public sealed class RecordingTokenIssuer : IAccessTokenIssuer
{
    public List<UserAccount> IssuedFor { get; } = [];

    public AccessToken Issue(UserAccount user)
    {
        IssuedFor.Add(user);
        return new AccessToken($"token-for-{user.Id}", FixedClock.Default.AddHours(1));
    }
}

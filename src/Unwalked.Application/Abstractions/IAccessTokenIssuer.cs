using Unwalked.Application.Auth;
using Unwalked.Domain.Users;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Issues access tokens for signed-in users.
/// </summary>
public interface IAccessTokenIssuer
{
    /// <summary>
    /// Creates a signed token for the user with its expiry time.
    /// </summary>
    AccessToken Issue(UserAccount user);
}

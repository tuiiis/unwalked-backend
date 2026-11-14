using Unwalked.Application.Auth;
using Unwalked.Domain.Users;

namespace Unwalked.Application.Abstractions;

public interface IAccessTokenIssuer
{
    AccessToken Issue(UserAccount user);
}

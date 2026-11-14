using Unwalked.Application.Abstractions;

namespace Unwalked.UnitTests.Fakes;

public sealed class PrefixPasswordHasher : IPasswordHasher
{
    private const string Prefix = "hashed:";

    public string Hash(string password) => Prefix + password;

    public bool Verify(string hash, string password) => hash == Prefix + password;
}

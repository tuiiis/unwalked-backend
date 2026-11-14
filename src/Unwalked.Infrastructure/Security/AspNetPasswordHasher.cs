using Microsoft.AspNetCore.Identity;
using Unwalked.Domain.Users;

namespace Unwalked.Infrastructure.Security;

public sealed class AspNetPasswordHasher : Application.Abstractions.IPasswordHasher
{
    private static readonly UserAccount HashSubject = new() { Email = string.Empty, PasswordHash = string.Empty };
    private readonly PasswordHasher<UserAccount> hasher = new();

    public string Hash(string password) => hasher.HashPassword(HashSubject, password);

    public bool Verify(string hash, string password) =>
        hasher.VerifyHashedPassword(HashSubject, hash, password) != PasswordVerificationResult.Failed;
}

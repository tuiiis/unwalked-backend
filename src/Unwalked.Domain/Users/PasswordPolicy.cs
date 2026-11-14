using Unwalked.Domain.Common;

namespace Unwalked.Domain.Users;

public static class PasswordPolicy
{
    public const int MinLength = 10;
    public const int MaxLength = 128;

    public static void Validate(string password)
    {
        if (password.Length is < MinLength or > MaxLength)
        {
            throw new DomainValidationException("password", $"Password must be {MinLength}-{MaxLength} characters long.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainValidationException("password", "Password must not be blank.");
        }
    }
}

using Unwalked.Domain.Common;

namespace Unwalked.Domain.Users;

/// <summary>
/// Password rules for new accounts.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 10;
    public const int MaxLength = 128;

    /// <summary>
    /// Throws DomainValidationException unless the password is 10 to 128 characters and not blank.
    /// </summary>
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

using System.Net.Mail;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Users;

/// <summary>
/// Validation of account email addresses.
/// </summary>
public static class EmailAddress
{
    public const int MaxLength = 254;

    /// <summary>
    /// Checks the address and returns it trimmed and lower-cased.
    /// </summary>
    public static string Normalize(string email)
    {
        var trimmed = email.Trim();
        if (trimmed.Length is 0 or > MaxLength
            || !MailAddress.TryCreate(trimmed, out var parsed)
            || parsed.Address != trimmed)
        {
            throw new DomainValidationException("email", "Email address is not valid.");
        }

        return trimmed.ToLowerInvariant();
    }
}

using System.Text.RegularExpressions;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Places;

/// <summary>
/// Validation of category codes sent by clients.
/// </summary>
public static partial class CategoryCode
{
    public const int MaxLength = 64;

    /// <summary>
    /// Trims and lower-cases the code and checks it uses only a-z, 0-9 and underscore.
    /// </summary>
    public static string Normalize(string code)
    {
        var normalized = code.Trim().ToLowerInvariant();
        if (!AllowedPattern().IsMatch(normalized))
        {
            throw new DomainValidationException("category", "Category must be 1-64 characters of a-z, 0-9 or underscore.");
        }

        return normalized;
    }

    /// <summary>
    /// Like Normalize, but returns null for an empty value.
    /// </summary>
    public static string? NormalizeOptional(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : Normalize(code);

    [GeneratedRegex("^[a-z0-9_]{1,64}$")]
    private static partial Regex AllowedPattern();
}

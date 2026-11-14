using System.Text.RegularExpressions;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Places;

public static partial class CategoryCode
{
    public const int MaxLength = 64;

    public static string Normalize(string code)
    {
        var normalized = code.Trim().ToLowerInvariant();
        if (!AllowedPattern().IsMatch(normalized))
        {
            throw new DomainValidationException("category", "Category must be 1-64 characters of a-z, 0-9 or underscore.");
        }

        return normalized;
    }

    public static string? NormalizeOptional(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : Normalize(code);

    [GeneratedRegex("^[a-z0-9_]{1,64}$")]
    private static partial Regex AllowedPattern();
}

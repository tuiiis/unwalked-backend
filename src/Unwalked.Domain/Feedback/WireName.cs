using System.Text;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Feedback;

/// <summary>
/// Converts enum values to and from the snake_case names used in the API.
/// </summary>
public static class WireName
{
    /// <summary>
    /// Returns the snake_case name of the value, for example NearbyNewPlace becomes nearby_new_place.
    /// </summary>
    public static string From<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        var name = value.ToString();
        var builder = new StringBuilder(name.Length + 4);
        foreach (var character in name)
        {
            if (char.IsUpper(character) && builder.Length > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Parses a snake_case name, ignoring case and surrounding spaces. Throws DomainValidationException listing the allowed names.
    /// </summary>
    public static TEnum Parse<TEnum>(string field, string text)
        where TEnum : struct, Enum
    {
        var normalized = text.Trim().ToLowerInvariant();
        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (From(value) == normalized)
            {
                return value;
            }
        }

        var allowed = string.Join(", ", Enum.GetValues<TEnum>().Select(From));
        throw new DomainValidationException(field, $"'{text}' is not one of: {allowed}.");
    }
}

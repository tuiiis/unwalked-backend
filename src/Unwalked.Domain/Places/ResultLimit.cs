using Unwalked.Domain.Common;

namespace Unwalked.Domain.Places;

/// <summary>
/// Rules for the number of results a search may return.
/// </summary>
public static class ResultLimit
{
    public const int Default = 50;
    public const int Max = 200;

    /// <summary>
    /// Returns the requested limit, or the default when none was given. Throws DomainValidationException outside 1 to 200.
    /// </summary>
    public static int Resolve(int? requested)
    {
        var limit = requested ?? Default;
        if (limit is < 1 or > Max)
        {
            throw new DomainValidationException("limit", $"Limit must be between 1 and {Max}.");
        }

        return limit;
    }
}

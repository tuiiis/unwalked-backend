using Unwalked.Domain.Common;

namespace Unwalked.Domain.Places;

public static class ResultLimit
{
    public const int Default = 50;
    public const int Max = 200;

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

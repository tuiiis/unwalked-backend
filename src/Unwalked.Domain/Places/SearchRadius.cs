using Unwalked.Domain.Common;

namespace Unwalked.Domain.Places;

public static class SearchRadius
{
    public const double MinMeters = 10;
    public const double MaxMeters = 5000;

    public static double Validate(double meters)
    {
        if (double.IsNaN(meters) || meters is < MinMeters or > MaxMeters)
        {
            throw new DomainValidationException("radius", $"Radius must be between {MinMeters} and {MaxMeters} metres.");
        }

        return meters;
    }
}

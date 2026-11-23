using Unwalked.Domain.Common;

namespace Unwalked.Domain.Places;

/// <summary>
/// Rules for the radius of a nearby search.
/// </summary>
public static class SearchRadius
{
    public const double MinMeters = 10;
    public const double MaxMeters = 5000;

    /// <summary>
    /// Returns the radius when it is between 10 and 5000 metres, otherwise throws DomainValidationException.
    /// </summary>
    public static double Validate(double meters)
    {
        if (double.IsNaN(meters) || meters is < MinMeters or > MaxMeters)
        {
            throw new DomainValidationException("radius", $"Radius must be between {MinMeters} and {MaxMeters} metres.");
        }

        return meters;
    }
}

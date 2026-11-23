using Unwalked.Domain.Common;

namespace Unwalked.Domain.Geo;

/// <summary>
/// A WGS84 point with validated latitude and longitude in degrees.
/// </summary>
public readonly record struct GeoCoordinate
{
    private GeoCoordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }

    /// <summary>
    /// Creates a point. Throws DomainValidationException when a value is out of range or not a number.
    /// </summary>
    public static GeoCoordinate Create(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || latitude is < -90 or > 90)
        {
            throw new DomainValidationException("lat", "Latitude must be between -90 and 90.");
        }

        if (double.IsNaN(longitude) || longitude is < -180 or > 180)
        {
            throw new DomainValidationException("lon", "Longitude must be between -180 and 180.");
        }

        return new GeoCoordinate(latitude, longitude);
    }
}

using Unwalked.Domain.Common;

namespace Unwalked.Domain.Geo;

public sealed record BoundingBox
{
    public const double MaxAreaSquareDegrees = 0.25;

    private BoundingBox(GeoCoordinate southWest, GeoCoordinate northEast)
    {
        SouthWest = southWest;
        NorthEast = northEast;
    }

    public GeoCoordinate SouthWest { get; }

    public GeoCoordinate NorthEast { get; }

    public static BoundingBox Create(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude)
    {
        var southWest = GeoCoordinate.Create(minLatitude, minLongitude);
        var northEast = GeoCoordinate.Create(maxLatitude, maxLongitude);

        if (southWest.Latitude >= northEast.Latitude || southWest.Longitude >= northEast.Longitude)
        {
            throw new DomainValidationException("bbox", "The minimum corner must be south-west of the maximum corner.");
        }

        var area = (northEast.Latitude - southWest.Latitude) * (northEast.Longitude - southWest.Longitude);
        if (area > MaxAreaSquareDegrees)
        {
            throw new DomainValidationException("bbox", $"The bounding box must not exceed {MaxAreaSquareDegrees} square degrees.");
        }

        return new BoundingBox(southWest, northEast);
    }
}

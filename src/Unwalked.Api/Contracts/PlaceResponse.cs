using Unwalked.Application.Places;
using Unwalked.Domain.Geo;

namespace Unwalked.Api.Contracts;

public sealed record PlaceResponse(
    long Id,
    string ExternalId,
    string Name,
    string Category,
    decimal Confidence,
    double Lat,
    double Lon,
    string H3Cell,
    double? DistanceMeters)
{
    public static PlaceResponse From(PlaceSummary place) => new(
        place.Id,
        place.ExternalId,
        place.Name,
        place.PrimaryCategory,
        place.Confidence,
        place.Latitude,
        place.Longitude,
        Domain.Geo.H3Cell.Format(place.H3Cell),
        place.DistanceMeters);
}

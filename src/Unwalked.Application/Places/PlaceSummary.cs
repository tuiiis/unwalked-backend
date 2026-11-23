namespace Unwalked.Application.Places;

/// <summary>
/// A place as returned by catalogue queries. Distance is set only for radius searches.
/// </summary>
public sealed record PlaceSummary(
    long Id,
    string ExternalId,
    string Name,
    string PrimaryCategory,
    decimal Confidence,
    double Latitude,
    double Longitude,
    long H3Cell,
    double? DistanceMeters);

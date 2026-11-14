namespace Unwalked.Application.Places;

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

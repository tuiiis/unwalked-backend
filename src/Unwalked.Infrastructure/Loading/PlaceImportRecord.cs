namespace Unwalked.Infrastructure.Loading;

public sealed record PlaceImportRecord(
    string ExternalId,
    string Name,
    string PrimaryCategory,
    IReadOnlyList<string> AlternateCategories,
    decimal Confidence,
    double Longitude,
    double Latitude);

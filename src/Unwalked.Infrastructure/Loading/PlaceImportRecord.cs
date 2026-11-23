namespace Unwalked.Infrastructure.Loading;

/// <summary>
/// One validated place row ready to be loaded.
/// </summary>
public sealed record PlaceImportRecord(
    string ExternalId,
    string Name,
    string PrimaryCategory,
    IReadOnlyList<string> AlternateCategories,
    decimal Confidence,
    double Longitude,
    double Latitude);

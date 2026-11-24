namespace Unwalked.Domain.Places;

/// <summary>
/// A place found in an open data source for an area that is loaded on demand.
/// </summary>
public sealed record AreaPlace(string ExternalId, string Name, string Category, double Latitude, double Longitude);

using NetTopologySuite.Geometries;
using Unwalked.Domain.Regions;

namespace Unwalked.Domain.Places;

/// <summary>
/// A point of interest from the Overture Maps catalogue.
/// </summary>
public sealed class Place
{
    public const int MaxNameLength = 300;
    public const int MaxExternalIdLength = 64;
    public const int MaxOpeningHoursLength = 255;

    public long Id { get; set; }

    /// <summary>
    /// Overture id, stable across catalogue releases.
    /// </summary>
    public required string ExternalId { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// Overture confidence that the place exists, from 0 to 1.
    /// </summary>
    public decimal Confidence { get; set; }

    public required Point Location { get; set; }

    /// <summary>
    /// Resolution 11 H3 cell of the location, used for cell searches.
    /// </summary>
    public long H3R11 { get; set; }

    public string? OpeningHours { get; set; }

    public int RegionId { get; set; }

    public Region? Region { get; set; }

    public int PrimaryCategoryId { get; set; }

    public Category? PrimaryCategory { get; set; }

    /// <summary>
    /// Additional categories that also match the place in searches.
    /// </summary>
    public List<PlaceAlternateCategory> AlternateCategories { get; set; } = [];
}

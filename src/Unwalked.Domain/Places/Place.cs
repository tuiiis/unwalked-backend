using NetTopologySuite.Geometries;
using Unwalked.Domain.Regions;

namespace Unwalked.Domain.Places;

public sealed class Place
{
    public const int MaxNameLength = 300;
    public const int MaxExternalIdLength = 64;
    public const int MaxOpeningHoursLength = 255;

    public long Id { get; set; }

    public required string ExternalId { get; set; }

    public required string Name { get; set; }

    public decimal Confidence { get; set; }

    public required Point Location { get; set; }

    public long H3R11 { get; set; }

    public string? OpeningHours { get; set; }

    public int RegionId { get; set; }

    public Region? Region { get; set; }

    public int PrimaryCategoryId { get; set; }

    public Category? PrimaryCategory { get; set; }

    public List<PlaceAlternateCategory> AlternateCategories { get; set; } = [];
}

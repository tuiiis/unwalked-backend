using NetTopologySuite.Geometries;
using Unwalked.Domain.Regions;

namespace Unwalked.Domain.Places;

/// <summary>
/// A building footprint from Overture Buildings. The phone uses footprints to decide which building a stay happened in.
/// </summary>
public sealed class Building
{
    public const int MaxExternalIdLength = 64;

    public long Id { get; set; }

    public required string ExternalId { get; set; }

    public required MultiPolygon Footprint { get; set; }

    /// <summary>
    /// Resolution 11 H3 cell of the footprint centre.
    /// </summary>
    public long H3R11 { get; set; }

    public int RegionId { get; set; }

    public Region? Region { get; set; }
}

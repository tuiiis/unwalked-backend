using NetTopologySuite.Geometries;

namespace Unwalked.Domain.Regions;

/// <summary>
/// A city area the catalogue covers, for example Vilnius.
/// </summary>
public sealed class Region
{
    public const int MaxCodeLength = 32;
    public const int MaxNameLength = 100;

    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// Outline of the region in WGS84.
    /// </summary>
    public required Polygon Bounds { get; set; }
}

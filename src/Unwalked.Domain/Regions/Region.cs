using NetTopologySuite.Geometries;

namespace Unwalked.Domain.Regions;

public sealed class Region
{
    public const int MaxCodeLength = 32;
    public const int MaxNameLength = 100;

    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public required Polygon Bounds { get; set; }
}

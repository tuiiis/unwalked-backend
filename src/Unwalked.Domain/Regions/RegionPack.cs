namespace Unwalked.Domain.Regions;

/// <summary>
/// A downloadable offline package of a region for the app.
/// </summary>
public sealed class RegionPack
{
    public const int Sha256HexLength = 64;
    public const int MaxObjectKeyLength = 200;

    public int Id { get; set; }

    public int RegionId { get; set; }

    public Region? Region { get; set; }

    public int Version { get; set; }

    public DateTimeOffset BuiltAt { get; set; }

    public long SizeBytes { get; set; }

    public required string Sha256 { get; set; }

    /// <summary>
    /// Key of the package file in object storage.
    /// </summary>
    public required string ObjectKey { get; set; }

    /// <summary>
    /// Whether this is the version the app should download. A unique index allows one current pack per region and a trigger retires the previous one.
    /// </summary>
    public bool IsCurrent { get; set; }
}

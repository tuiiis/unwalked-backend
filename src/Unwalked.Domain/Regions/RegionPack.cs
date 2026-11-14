namespace Unwalked.Domain.Regions;

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

    public required string ObjectKey { get; set; }

    public bool IsCurrent { get; set; }
}

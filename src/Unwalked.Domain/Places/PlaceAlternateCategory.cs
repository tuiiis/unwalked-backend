namespace Unwalked.Domain.Places;

/// <summary>
/// Link between a place and one of its alternate categories.
/// </summary>
public sealed class PlaceAlternateCategory
{
    public long PlaceId { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}

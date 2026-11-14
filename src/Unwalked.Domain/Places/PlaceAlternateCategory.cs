namespace Unwalked.Domain.Places;

public sealed class PlaceAlternateCategory
{
    public long PlaceId { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}

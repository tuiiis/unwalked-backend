namespace Unwalked.Domain.Places;

/// <summary>
/// A place category from the Overture taxonomy, for example cafe or park.
/// </summary>
public sealed class Category
{
    public int Id { get; set; }

    public required string Code { get; set; }
}

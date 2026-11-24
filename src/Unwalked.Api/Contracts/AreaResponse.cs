using Unwalked.Application.Areas;

namespace Unwalked.Api.Contracts;

/// <summary>
/// State of an area of the catalogue. Region is set when the area is ready.
/// </summary>
public sealed record AreaResponse(string Cell, AreaStatus Status, string? Region)
{
    /// <summary>
    /// Maps an area state to the wire format.
    /// </summary>
    public static AreaResponse From(AreaState state) => new(state.Cell, state.Status, state.Region);
}

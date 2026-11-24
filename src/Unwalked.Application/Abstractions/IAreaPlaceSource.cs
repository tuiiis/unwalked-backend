using H3;
using Unwalked.Domain.Places;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Open data source of places for an area.
/// </summary>
public interface IAreaPlaceSource
{
    /// <summary>
    /// Named places inside the cell with a category from the catalogue.
    /// </summary>
    Task<IReadOnlyList<AreaPlace>> FetchAsync(H3Index cell, CancellationToken cancellationToken);
}

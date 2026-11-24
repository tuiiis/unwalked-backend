using H3;
using Unwalked.Domain.Places;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Areas of the catalogue and the places loaded for them.
/// </summary>
public interface IAreaRepository
{
    /// <summary>
    /// Code of a region whose bounds overlap the cell, or null.
    /// </summary>
    Task<string?> FindCoveringRegionAsync(H3Index cell, CancellationToken cancellationToken);

    /// <summary>
    /// Number of regions loaded on demand.
    /// </summary>
    Task<int> CountLoadedAreasAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stores the area as a region together with its places in one transaction and returns the region code.
    /// </summary>
    Task<string> SaveAsync(H3Index cell, IReadOnlyList<AreaPlace> places, CancellationToken cancellationToken);
}

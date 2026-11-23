using Unwalked.Application.Places;
using Unwalked.Domain.Geo;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Read-only queries against the place catalogue.
/// </summary>
public interface IPlaceReadRepository
{
    /// <summary>
    /// Places within the radius of a point, nearest first.
    /// </summary>
    Task<IReadOnlyList<PlaceSummary>> FindNearAsync(GeoCoordinate center, double radiusMeters, string? category, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Places inside any of the given resolution 11 H3 cells.
    /// </summary>
    Task<IReadOnlyList<PlaceSummary>> FindInCellsAsync(IReadOnlyCollection<long> cells, string? category, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Places inside the bounding box, most confident first.
    /// </summary>
    Task<IReadOnlyList<PlaceSummary>> FindInBoundingBoxAsync(BoundingBox box, string? category, int limit, CancellationToken cancellationToken);
}

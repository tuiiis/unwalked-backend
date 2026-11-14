using Unwalked.Application.Places;
using Unwalked.Domain.Geo;

namespace Unwalked.Application.Abstractions;

public interface IPlaceReadRepository
{
    Task<IReadOnlyList<PlaceSummary>> FindNearAsync(GeoCoordinate center, double radiusMeters, string? category, int limit, CancellationToken cancellationToken);

    Task<IReadOnlyList<PlaceSummary>> FindInCellsAsync(IReadOnlyCollection<long> cells, string? category, int limit, CancellationToken cancellationToken);

    Task<IReadOnlyList<PlaceSummary>> FindInBoundingBoxAsync(BoundingBox box, string? category, int limit, CancellationToken cancellationToken);
}

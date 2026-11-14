using Unwalked.Application.Abstractions;
using Unwalked.Application.Places;
using Unwalked.Domain.Geo;

namespace Unwalked.UnitTests.Fakes;

public sealed class RecordingPlaceReadRepository : IPlaceReadRepository
{
    private static readonly IReadOnlyList<PlaceSummary> NoPlaces = [];

    public GeoCoordinate? LastCenter { get; private set; }

    public double? LastRadius { get; private set; }

    public BoundingBox? LastBox { get; private set; }

    public IReadOnlyCollection<long>? LastCells { get; private set; }

    public string? LastCategory { get; private set; }

    public int? LastLimit { get; private set; }

    public Task<IReadOnlyList<PlaceSummary>> FindNearAsync(GeoCoordinate center, double radiusMeters, string? category, int limit, CancellationToken cancellationToken)
    {
        (LastCenter, LastRadius, LastCategory, LastLimit) = (center, radiusMeters, category, limit);
        return Task.FromResult(NoPlaces);
    }

    public Task<IReadOnlyList<PlaceSummary>> FindInCellsAsync(IReadOnlyCollection<long> cells, string? category, int limit, CancellationToken cancellationToken)
    {
        (LastCells, LastCategory, LastLimit) = (cells, category, limit);
        return Task.FromResult(NoPlaces);
    }

    public Task<IReadOnlyList<PlaceSummary>> FindInBoundingBoxAsync(BoundingBox box, string? category, int limit, CancellationToken cancellationToken)
    {
        (LastBox, LastCategory, LastLimit) = (box, category, limit);
        return Task.FromResult(NoPlaces);
    }
}

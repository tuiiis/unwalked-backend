using Unwalked.Application.Abstractions;
using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;

namespace Unwalked.Application.Places;

/// <summary>
/// Place search use cases. Validates the input and passes normalised queries to the repository.
/// </summary>
public sealed class PlaceQueryService(IPlaceReadRepository places)
{
    /// <summary>
    /// Most H3 cells a client may send in one request.
    /// </summary>
    public const int MaxRequestedCells = 500;
    /// <summary>
    /// Most resolution 11 cells one request may cover after expansion.
    /// </summary>
    public const int MaxExpandedCells = 20_000;

    /// <summary>
    /// Places within a radius of a point.
    /// </summary>
    public Task<IReadOnlyList<PlaceSummary>> FindNearAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        string? category,
        int? limit,
        CancellationToken cancellationToken)
    {
        var center = GeoCoordinate.Create(latitude, longitude);
        var radius = SearchRadius.Validate(radiusMeters);
        return places.FindNearAsync(center, radius, CategoryCode.NormalizeOptional(category), ResultLimit.Resolve(limit), cancellationToken);
    }

    /// <summary>
    /// Places inside a bounding box.
    /// </summary>
    public Task<IReadOnlyList<PlaceSummary>> FindInBoundingBoxAsync(
        double minLatitude,
        double minLongitude,
        double maxLatitude,
        double maxLongitude,
        string? category,
        int? limit,
        CancellationToken cancellationToken)
    {
        var box = BoundingBox.Create(minLatitude, minLongitude, maxLatitude, maxLongitude);
        return places.FindInBoundingBoxAsync(box, CategoryCode.NormalizeOptional(category), ResultLimit.Resolve(limit), cancellationToken);
    }

    /// <summary>
    /// Places inside the given H3 cells of any supported resolution.
    /// </summary>
    public Task<IReadOnlyList<PlaceSummary>> FindInCellsAsync(
        IReadOnlyCollection<string> cells,
        string? category,
        int? limit,
        CancellationToken cancellationToken)
    {
        var expanded = ExpandCells(cells);
        return places.FindInCellsAsync(expanded, CategoryCode.NormalizeOptional(category), ResultLimit.Resolve(limit), cancellationToken);
    }

    /// <summary>
    /// Parses the cells and expands each one to its resolution 11 children, enforcing the request limits.
    /// </summary>
    public static IReadOnlyCollection<long> ExpandCells(IReadOnlyCollection<string> cells)
    {
        if (cells.Count is 0 or > MaxRequestedCells)
        {
            throw new DomainValidationException("cells", $"Provide between 1 and {MaxRequestedCells} cells.");
        }

        var expanded = new HashSet<long>();
        foreach (var cell in cells)
        {
            foreach (var child in H3Cell.ExpandToPlaceResolution(H3Cell.Parse(cell)))
            {
                expanded.Add(child);
                if (expanded.Count > MaxExpandedCells)
                {
                    throw new DomainValidationException("cells", $"The cells cover more than {MaxExpandedCells} resolution {H3Cell.PlaceResolution} cells.");
                }
            }
        }

        return expanded;
    }
}

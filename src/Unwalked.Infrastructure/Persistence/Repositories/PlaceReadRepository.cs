using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Unwalked.Application.Abstractions;
using Unwalked.Application.Places;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Repositories;

/// <summary>
/// Catalogue queries with PostGIS. Radius searches use geography distance, so results are in metres.
/// </summary>
public sealed class PlaceReadRepository(UnwalkedDbContext db, GeometryFactory geometryFactory) : IPlaceReadRepository
{
    public async Task<IReadOnlyList<PlaceSummary>> FindNearAsync(GeoCoordinate center, double radiusMeters, string? category, int limit, CancellationToken cancellationToken)
    {
        var point = ToPoint(center);
        var rows = await FilterByCategory(db.Places, category)
            .OrderBy(place => EF.Functions.DistanceKnn(place.Location, point))
            .Take(limit)
            .Where(place => place.Location.IsWithinDistance(point, radiusMeters))
            .OrderBy(place => EF.Functions.DistanceKnn(place.Location, point))
            .Select(place => new PlaceRow(
                place.Id,
                place.ExternalId,
                place.Name,
                place.PrimaryCategory!.Code,
                place.Confidence,
                place.Location,
                place.H3R11,
                place.Location.Distance(point)))
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(row => row.ToSummary());
    }

    public async Task<IReadOnlyList<PlaceSummary>> FindInCellsAsync(IReadOnlyCollection<long> cells, string? category, int limit, CancellationToken cancellationToken)
    {
        var cellArray = cells.ToArray();
        var rows = await FilterByCategory(db.Places.Where(place => cellArray.Contains(place.H3R11)), category)
            .OrderByDescending(place => place.Confidence)
            .ThenBy(place => place.Id)
            .Take(limit)
            .Select(ToRowWithoutDistance())
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(row => row.ToSummary());
    }

    public async Task<IReadOnlyList<PlaceSummary>> FindInBoundingBoxAsync(BoundingBox box, string? category, int limit, CancellationToken cancellationToken)
    {
        var envelope = geometryFactory.ToGeometry(new Envelope(
            box.SouthWest.Longitude,
            box.NorthEast.Longitude,
            box.SouthWest.Latitude,
            box.NorthEast.Latitude));

        var rows = await FilterByCategory(db.Places.Where(place => place.Location.Intersects(envelope)), category)
            .OrderByDescending(place => place.Confidence)
            .ThenBy(place => place.Id)
            .Take(limit)
            .Select(ToRowWithoutDistance())
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(row => row.ToSummary());
    }

    private IQueryable<Place> FilterByCategory(IQueryable<Place> places, string? category)
    {
        places = places.AsNoTracking();
        return category is null
            ? places
            : places.Where(place =>
                place.PrimaryCategory!.Code == category
                || place.AlternateCategories.Any(alternate => alternate.Category!.Code == category));
    }

    private static Expression<Func<Place, PlaceRow>> ToRowWithoutDistance() =>
        place => new PlaceRow(
            place.Id,
            place.ExternalId,
            place.Name,
            place.PrimaryCategory!.Code,
            place.Confidence,
            place.Location,
            place.H3R11,
            null);

    private Point ToPoint(GeoCoordinate coordinate) =>
        geometryFactory.CreatePoint(new Coordinate(coordinate.Longitude, coordinate.Latitude));

    private sealed record PlaceRow(
        long Id,
        string ExternalId,
        string Name,
        string PrimaryCategory,
        decimal Confidence,
        Point Location,
        long H3R11,
        double? DistanceMeters)
    {
        public PlaceSummary ToSummary() => new(
            Id,
            ExternalId,
            Name,
            PrimaryCategory,
            Confidence,
            Location.Y,
            Location.X,
            H3R11,
            DistanceMeters is null ? null : Math.Round(DistanceMeters.Value, 1));
    }
}

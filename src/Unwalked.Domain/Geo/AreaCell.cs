using H3;
using H3.Extensions;
using H3.Model;
using NetTopologySuite.Geometries;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Geo;

/// <summary>
/// H3 cells of resolution 5, about 250 square kilometres each, used as the unit in which the catalogue grows outside the preloaded regions.
/// </summary>
public static class AreaCell
{
    /// <summary>
    /// Resolution of an area cell.
    /// </summary>
    public const int Resolution = 5;

    /// <summary>
    /// Prefix of the region code that marks a region loaded on demand.
    /// </summary>
    public const string RegionPrefix = "area-";

    /// <summary>
    /// Parses a resolution 5 cell. Throws DomainValidationException for anything else.
    /// </summary>
    public static H3Index Parse(string text)
    {
        var index = H3Cell.Parse(text);
        if (index.Resolution != Resolution)
        {
            throw new DomainValidationException("cell", $"An area is an H3 cell of resolution {Resolution}.");
        }

        return index;
    }

    /// <summary>
    /// The area cell that contains the point.
    /// </summary>
    public static H3Index ForCoordinate(GeoCoordinate coordinate) =>
        H3Index.FromLatLng(LatLng.FromCoordinate(new Coordinate(coordinate.Longitude, coordinate.Latitude)), Resolution);

    /// <summary>
    /// Region code under which an area is stored, for example area-851f1d4bfffffff.
    /// </summary>
    public static string RegionCode(H3Index cell) => RegionPrefix + cell;

    /// <summary>
    /// The outline of the cell as a WGS84 polygon.
    /// </summary>
    public static Polygon Outline(H3Index cell, GeometryFactory factory) => (Polygon)factory.CreateGeometry(cell.GetCellBoundary());

    /// <summary>
    /// The centre of the cell.
    /// </summary>
    public static GeoCoordinate Centre(H3Index cell)
    {
        var centre = cell.ToLatLng();
        return GeoCoordinate.Create(centre.LatitudeDegrees, centre.LongitudeDegrees);
    }
}

using H3;
using H3.Extensions;
using H3.Model;
using NetTopologySuite.Geometries;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Geo;

/// <summary>
/// Helpers for the Uber H3 grid that indexes places.
/// </summary>
public static class H3Cell
{
    /// <summary>
    /// Resolution every place is indexed at, hexagons of about 25 m edge.
    /// </summary>
    public const int PlaceResolution = 11;
    /// <summary>
    /// Coarsest resolution a client may query with.
    /// </summary>
    public const int CoarsestQueryResolution = 7;

    /// <summary>
    /// Returns the resolution 11 cell that contains the point.
    /// </summary>
    public static long ForCoordinate(GeoCoordinate coordinate)
    {
        var latLng = LatLng.FromCoordinate(new Coordinate(coordinate.Longitude, coordinate.Latitude));
        return (long)(ulong)H3Index.FromLatLng(latLng, PlaceResolution);
    }

    /// <summary>
    /// Formats a cell as the usual 15 character hex string.
    /// </summary>
    public static string Format(long cell) => new H3Index((ulong)cell).ToString();

    /// <summary>
    /// Parses a hex cell id. Throws DomainValidationException when it is not a valid cell.
    /// </summary>
    public static H3Index Parse(string text)
    {
        H3Index index;
        try
        {
            index = new H3Index(text.Trim());
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or ArgumentException)
        {
            throw new DomainValidationException("cells", $"'{text}' is not an H3 cell.");
        }

        if (!index.IsValidCell)
        {
            throw new DomainValidationException("cells", $"'{text}' is not an H3 cell.");
        }

        return index;
    }

    /// <summary>
    /// Returns the resolution 11 cells inside the given cell, or the cell itself when it already is resolution 11.
    /// </summary>
    public static IEnumerable<long> ExpandToPlaceResolution(H3Index cell)
    {
        if (cell.Resolution is < CoarsestQueryResolution or > PlaceResolution)
        {
            throw new DomainValidationException(
                "cells",
                $"Cell resolution must be between {CoarsestQueryResolution} and {PlaceResolution}.");
        }

        return cell.Resolution == PlaceResolution
            ? new[] { (long)(ulong)cell }
            : cell.GetChildrenForResolution(PlaceResolution).Select(child => (long)(ulong)child);
    }
}

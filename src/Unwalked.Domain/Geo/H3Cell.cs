using H3;
using H3.Extensions;
using H3.Model;
using NetTopologySuite.Geometries;
using Unwalked.Domain.Common;

namespace Unwalked.Domain.Geo;

public static class H3Cell
{
    public const int PlaceResolution = 11;
    public const int CoarsestQueryResolution = 7;

    public static long ForCoordinate(GeoCoordinate coordinate)
    {
        var latLng = LatLng.FromCoordinate(new Coordinate(coordinate.Longitude, coordinate.Latitude));
        return (long)(ulong)H3Index.FromLatLng(latLng, PlaceResolution);
    }

    public static string Format(long cell) => new H3Index((ulong)cell).ToString();

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

using System.Globalization;
using System.Text;
using System.Text.Json;
using H3;
using NetTopologySuite.Geometries;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Areas;

/// <summary>
/// Reads named places of an area from OpenStreetMap through the Overpass API.
/// </summary>
public sealed class OverpassPlaceSource(HttpClient http, GeometryFactory geometryFactory) : IAreaPlaceSource
{
    /// <summary>
    /// Name of the HTTP client configured for Overpass.
    /// </summary>
    public const string ClientName = "overpass";

    /// <summary>
    /// Sends one Overpass query for the cell's bounding box and keeps the places inside the cell.
    /// </summary>
    public async Task<IReadOnlyList<AreaPlace>> FetchAsync(H3Index cell, CancellationToken cancellationToken)
    {
        var outline = AreaCell.Outline(cell, geometryFactory);
        var query = Query(outline.EnvelopeInternal);
        for (var attempt = 0; ; attempt++)
        {
            using var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("data", query)]);
            using var response = await http.PostAsync(string.Empty, content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
                return Parse(await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken), outline);
            }

            if (attempt >= Backoff.Length || !Busy.Contains(response.StatusCode))
            {
                response.EnsureSuccessStatusCode();
            }

            await Task.Delay(Backoff[attempt], cancellationToken);
        }
    }

    /// <summary>
    /// Waits between attempts while Overpass answers that it is busy.
    /// </summary>
    public static TimeSpan[] Backoff { get; set; } = [TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(90)];

    private static readonly HashSet<System.Net.HttpStatusCode> Busy =
    [
        System.Net.HttpStatusCode.TooManyRequests,
        System.Net.HttpStatusCode.BadGateway,
        System.Net.HttpStatusCode.ServiceUnavailable,
        System.Net.HttpStatusCode.GatewayTimeout,
    ];

    /// <summary>
    /// Turns an Overpass JSON answer into places, keeping named elements with a known category inside the outline.
    /// </summary>
    public static IReadOnlyList<AreaPlace> Parse(JsonDocument answer, Polygon outline)
    {
        var places = new List<AreaPlace>();
        foreach (var element in answer.RootElement.GetProperty("elements").EnumerateArray())
        {
            if (!element.TryGetProperty("tags", out var tagsElement))
            {
                continue;
            }

            var tags = tagsElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetString() ?? string.Empty);
            if (!tags.TryGetValue("name", out var name) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var category = OsmCategories.For(tags);
            var position = element.TryGetProperty("center", out var center) ? center : element;
            if (category is null || !position.TryGetProperty("lat", out var lat) || !position.TryGetProperty("lon", out var lon))
            {
                continue;
            }

            if (!outline.Contains(outline.Factory.CreatePoint(new Coordinate(lon.GetDouble(), lat.GetDouble()))))
            {
                continue;
            }

            var id = $"osm:{element.GetProperty("type").GetString()}/{element.GetProperty("id").GetInt64()}";
            places.Add(new AreaPlace(id, name.Trim()[..Math.Min(name.Trim().Length, 300)], category, lat.GetDouble(), lon.GetDouble()));
        }

        return places;
    }

    private static string Query(Envelope box)
    {
        var bbox = string.Create(CultureInfo.InvariantCulture, $"{box.MinY},{box.MinX},{box.MaxY},{box.MaxX}");
        var query = new StringBuilder("[out:json][timeout:120];(");
        foreach (var key in OsmCategories.Keys)
        {
            query.Append(CultureInfo.InvariantCulture, $"nwr[\"name\"][\"{key}\"~\"^({string.Join('|', OsmCategories.Values(key))})$\"]({bbox});");
        }

        return query.Append(");out center tags;").ToString();
    }
}

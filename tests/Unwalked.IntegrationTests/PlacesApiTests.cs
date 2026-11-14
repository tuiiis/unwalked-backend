using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using H3.Extensions;
using Unwalked.Domain.Geo;

namespace Unwalked.IntegrationTests;

public class PlacesApiTests : ApiTestBase
{
    private static readonly string Cathedral = string.Create(
        CultureInfo.InvariantCulture,
        $"lat={FixturePlaces.CathedralLatitude}&lon={FixturePlaces.CathedralLongitude}");

    [Test]
    public async Task Near_ReturnsPlacesInsideRadiusNearestFirst()
    {
        var places = await GetPlacesAsync($"/api/v1/places/near?{Cathedral}&radius=500");

        Assert.Multiple(() =>
        {
            Assert.That(ExternalIds(places), Is.EqualTo(new[] { "cathedral", "cafe-near", "museum-old-town" }));
            Assert.That(places.Select(place => place!["distanceMeters"]!.GetValue<double>()), Is.Ordered.And.All.LessThanOrEqualTo(500));
        });
    }

    [Test]
    public async Task Near_MatchesCategoryOnPrimaryOrAlternate()
    {
        var places = await GetPlacesAsync($"/api/v1/places/near?{Cathedral}&radius=3000&category=CAFE");

        Assert.That(ExternalIds(places), Is.EqualTo(new[] { "cafe-near", "museum-old-town", "cafe-antakalnis" }));
    }

    [Test]
    public async Task Near_RespectsLimit()
    {
        var places = await GetPlacesAsync($"/api/v1/places/near?{Cathedral}&radius=5000&limit=2");

        Assert.That(ExternalIds(places), Is.EqualTo(new[] { "cathedral", "cafe-near" }));
    }

    [Test]
    public async Task Near_ReturnsResolution11CellOfThePlace()
    {
        var places = await GetPlacesAsync($"/api/v1/places/near?{Cathedral}&radius=10&limit=1");

        var expected = H3Cell.Format(H3Cell.ForCoordinate(GeoCoordinate.Create(FixturePlaces.CathedralLatitude, FixturePlaces.CathedralLongitude)));
        Assert.That(places.Single()!["h3Cell"]!.GetValue<string>(), Is.EqualTo(expected));
    }

    [TestCase("radius=9000", "radius")]
    [TestCase("radius=500&limit=0", "limit")]
    [TestCase("radius=500&category=no%20spaces", "category")]
    public async Task Near_RejectsInvalidArgumentsWithValidationProblem(string query, string field)
    {
        var response = await Client.GetAsync($"/api/v1/places/near?{Cathedral}&{query}");

        var problem = await ReadJsonAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(problem["errors"]![field], Is.Not.Null);
            Assert.That(problem["traceId"], Is.Not.Null);
        });
    }

    [Test]
    public async Task Near_RejectsMissingCoordinateInsteadOfDefaultingToZero()
    {
        var response = await Client.GetAsync("/api/v1/places/near?lon=25.28&radius=500");

        var problem = await ReadJsonAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problem["errors"]!["lat"], Is.Not.Null);
        });
    }

    [Test]
    public async Task InBoundingBox_ReturnsPlacesInsideOrderedByConfidence()
    {
        var places = await GetPlacesAsync("/api/v1/places/in-bbox?minLat=54.68&minLon=25.28&maxLat=54.69&maxLon=25.30");

        Assert.That(ExternalIds(places), Is.EqualTo(new[] { "cathedral", "cafe-near", "museum-old-town", "bakery-uzupis" }));
    }

    [Test]
    public async Task ByCells_FindsPlacesInsideCoarserCell()
    {
        var cathedral = GeoCoordinate.Create(FixturePlaces.CathedralLatitude, FixturePlaces.CathedralLongitude);
        var parent = H3Cell.Parse(H3Cell.Format(H3Cell.ForCoordinate(cathedral))).GetParentForResolution(9).ToString();

        var response = await Client.PostAsJsonAsync("/api/v1/places/by-cells", new { cells = new[] { parent } });
        var places = (await ReadJsonAsync(response)).AsArray();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(ExternalIds(places), Does.Contain("cathedral"));
            Assert.That(ExternalIds(places), Does.Not.Contain("park-vingis"));
        });
    }

    [Test]
    public async Task ByCells_RejectsTextThatIsNotACell()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/places/by-cells", new { cells = new[] { "nope" } });

        var problem = await ReadJsonAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problem["errors"]!["cells"]![0]!.GetValue<string>(), Is.EqualTo("'nope' is not an H3 cell."));
        });
    }

    private async Task<JsonArray> GetPlacesAsync(string url)
    {
        var response = await Client.GetAsync(url);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
        return (await ReadJsonAsync(response)).AsArray();
    }

    private static List<string> ExternalIds(JsonArray places) =>
        places.Select(place => place!["externalId"]!.GetValue<string>()).ToList();
}

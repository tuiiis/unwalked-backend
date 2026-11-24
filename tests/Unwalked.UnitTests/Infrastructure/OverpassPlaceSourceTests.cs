using System.Text.Json;
using NetTopologySuite;
using Unwalked.Domain.Geo;
using Unwalked.Infrastructure.Areas;

namespace Unwalked.UnitTests.Infrastructure;

public class OverpassPlaceSourceTests
{
    [Test]
    public void KeepsNamedPlacesWithAKnownCategoryInsideTheArea()
    {
        var centre = GeoCoordinate.Create(54.8985, 23.9036);
        var cell = AreaCell.ForCoordinate(centre);
        var outline = AreaCell.Outline(cell, NtsGeometryServices.Instance.CreateGeometryFactory(4326));
        var lat = centre.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lon = centre.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        using var answer = JsonDocument.Parse($$$"""
            {"elements": [
              {"type": "node", "id": 1, "lat": {{{lat}}}, "lon": {{{lon}}}, "tags": {"name": "Kavinė", "amenity": "cafe"}},
              {"type": "way", "id": 2, "center": {"lat": {{{lat}}}, "lon": {{{lon}}}}, "tags": {"name": "Ąžuolynas", "leisure": "park"}},
              {"type": "node", "id": 3, "lat": {{{lat}}}, "lon": {{{lon}}}, "tags": {"name": "Šv. Petro bažnyčia", "amenity": "place_of_worship", "religion": "christian"}},
              {"type": "node", "id": 4, "lat": {{{lat}}}, "lon": {{{lon}}}, "tags": {"amenity": "cafe"}},
              {"type": "node", "id": 5, "lat": {{{lat}}}, "lon": {{{lon}}}, "tags": {"name": "Degalinė", "amenity": "fuel"}},
              {"type": "node", "id": 6, "lat": 10.0, "lon": 10.0, "tags": {"name": "Far away", "amenity": "cafe"}}
            ]}
            """);

        var places = OverpassPlaceSource.Parse(answer, outline);

        Assert.Multiple(() =>
        {
            Assert.That(places.Select(place => place.ExternalId), Is.EqualTo(new[] { "osm:node/1", "osm:way/2", "osm:node/3" }));
            Assert.That(places.Select(place => place.Category), Is.EqualTo(new[] { "cafe", "park", "christian_place_of_worship" }));
        });
    }

    [TestCase("amenity", "fast_food", "fast_food_restaurant")]
    [TestCase("shop", "books", "bookstore")]
    [TestCase("historic", "memorial", "monument")]
    [TestCase("shop", "car_repair", null)]
    public void MapsTagsToCatalogueCategories(string key, string value, string? expected)
    {
        Assert.That(OsmCategories.For(new Dictionary<string, string> { [key] = value }), Is.EqualTo(expected));
    }

    [Test]
    public async Task RetriesWhileOverpassIsBusy()
    {
        var answers = new Queue<HttpResponseMessage>(
        [
            new HttpResponseMessage(System.Net.HttpStatusCode.GatewayTimeout),
            new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("""{"elements":[]}""") },
        ]);
        var saved = OverpassPlaceSource.Backoff;
        OverpassPlaceSource.Backoff = [TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero];
        try
        {
            var source = new OverpassPlaceSource(new HttpClient(new Scripted(answers)) { BaseAddress = new Uri("http://overpass.test/") }, NtsGeometryServices.Instance.CreateGeometryFactory(4326));

            var places = await source.FetchAsync(AreaCell.ForCoordinate(GeoCoordinate.Create(54.8985, 23.9036)), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(places, Is.Empty);
                Assert.That(answers, Is.Empty);
            });
        }
        finally
        {
            OverpassPlaceSource.Backoff = saved;
        }
    }

    [Test]
    public void GivesUpOnAnError()
    {
        var answers = new Queue<HttpResponseMessage>([new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)]);
        var source = new OverpassPlaceSource(new HttpClient(new Scripted(answers)) { BaseAddress = new Uri("http://overpass.test/") }, NtsGeometryServices.Instance.CreateGeometryFactory(4326));

        Assert.ThrowsAsync<HttpRequestException>(() => source.FetchAsync(AreaCell.ForCoordinate(GeoCoordinate.Create(54.8985, 23.9036)), CancellationToken.None));
    }

    private sealed class Scripted(Queue<HttpResponseMessage> answers) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(answers.Dequeue());
    }
}

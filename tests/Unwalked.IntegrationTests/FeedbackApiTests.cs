using System.Net;
using System.Net.Http.Json;

namespace Unwalked.IntegrationTests;

public class FeedbackApiTests : ApiTestBase
{
    [Test]
    public async Task Submit_StoresOnceAndCountsInStats()
    {
        var accepted = new { eventId = Guid.NewGuid(), recommendationType = "along_habitual_route", action = "accepted", category = "cafe" };
        var dismissed = new { eventId = Guid.NewGuid(), recommendationType = "along_habitual_route", action = "dismissed", category = (string?)null };

        var before = await RouteStatsAsync();
        var first = await Client.PostAsJsonAsync("/api/v1/feedback", accepted);
        var repeat = await Client.PostAsJsonAsync("/api/v1/feedback", accepted);
        await Client.PostAsJsonAsync("/api/v1/feedback", dismissed);
        var after = await RouteStatsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(repeat.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(after.Accepted - before.Accepted, Is.EqualTo(1));
            Assert.That(after.Dismissed - before.Dismissed, Is.EqualTo(1));
        });
    }

    [TestCase("teleport", "accepted", null, "recommendationType")]
    [TestCase("walking_route", "accepted", "not_a_category", "category")]
    public async Task Submit_RejectsUnknownValues(string type, string action, string? category, string field)
    {
        var response = await Client.PostAsJsonAsync("/api/v1/feedback", new { eventId = Guid.NewGuid(), recommendationType = type, action, category });

        var problem = await ReadJsonAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problem["errors"]![field], Is.Not.Null);
        });
    }

    [Test]
    public async Task Stats_RejectsInvertedRange()
    {
        var response = await Client.GetAsync("/api/v1/feedback/stats?from=2026-10-10&to=2026-10-01");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private async Task<(long Accepted, long Dismissed)> RouteStatsAsync()
    {
        var stats = await ReadJsonAsync(await Client.GetAsync("/api/v1/feedback/stats"));
        var route = stats["types"]!.AsArray().SingleOrDefault(row => row!["recommendationType"]!.GetValue<string>() == "along_habitual_route");
        return route is null ? (0, 0) : (route["accepted"]!.GetValue<long>(), route["dismissed"]!.GetValue<long>());
    }
}

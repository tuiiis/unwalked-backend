using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using H3;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;
using Unwalked.Infrastructure.Areas;

namespace Unwalked.IntegrationTests;

public class AreasApiTests
{
    private static readonly GeoCoordinate KaunasCentre = GeoCoordinate.Create(54.8985, 23.9036);

    private sealed class FixedSource : IAreaPlaceSource
    {
        public Task<IReadOnlyList<AreaPlace>> FetchAsync(H3Index cell, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AreaPlace>>(
            [
                new AreaPlace("osm:node/1", "Laisvės alėjos kavinė", "cafe", KaunasCentre.Latitude, KaunasCentre.Longitude),
                new AreaPlace("osm:way/2", "Unknown kind", "not_a_category", KaunasCentre.Latitude, KaunasCentre.Longitude),
            ]);
    }

    private sealed class BrokenSource : IAreaPlaceSource
    {
        public Task<IReadOnlyList<AreaPlace>> FetchAsync(H3Index cell, CancellationToken cancellationToken) =>
            throw new HttpRequestException("Overpass is down");
    }

    private sealed class FactoryWith(IAreaPlaceSource source) : UnwalkedApiFactory
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services => services.AddScoped(_ => source));
        }
    }

    [Test]
    public async Task AreaOutsideTheCatalogueIsLoadedInTheBackground()
    {
        await using var factory = new FactoryWith(new FixedSource());
        using var client = factory.CreateClient();
        var cell = AreaCell.ForCoordinate(KaunasCentre);
        var near = string.Create(CultureInfo.InvariantCulture, $"/api/v1/places/near?lat={KaunasCentre.Latitude}&lon={KaunasCentre.Longitude}&radius=100");
        var nearBefore = await client.GetFromJsonAsync<List<PlaceBody>>(near);

        var started = await client.PutAsync($"/api/v1/areas/{cell}", null);
        await factory.Services.GetRequiredService<BackgroundAreaLoader>().WaitAsync(cell);
        var state = await client.GetFromJsonAsync<AreaBody>($"/api/v1/areas/{cell}");
        var again = await client.PutAsync($"/api/v1/areas/{cell}", null);
        var places = await client.PostAsJsonAsync("/api/v1/places/by-cells", new { cells = new[] { H3Cell.Format(H3Cell.ForCoordinate(KaunasCentre)) } });
        var found = await places.Content.ReadFromJsonAsync<List<PlaceBody>>();
        var nearAfter = await client.GetFromJsonAsync<List<PlaceBody>>(near);

        Assert.Multiple(() =>
        {
            Assert.That(started.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(state!.Status, Is.EqualTo("Ready"));
            Assert.That(state.Region, Is.EqualTo($"area-{cell}"));
            Assert.That(again.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(found!.Select(place => place.ExternalId), Is.EqualTo(new[] { "osm:node/1" }));
            Assert.That(nearBefore, Is.Empty);
            Assert.That(nearAfter!.Select(place => place.ExternalId), Is.EqualTo(new[] { "osm:node/1" }));
        });
    }

    [Test]
    public async Task AreaInsideAPreloadedRegionIsReadyAtOnce()
    {
        await using var factory = new FactoryWith(new BrokenSource());
        using var client = factory.CreateClient();
        var cell = AreaCell.ForCoordinate(GeoCoordinate.Create(FixturePlaces.CathedralLatitude, FixturePlaces.CathedralLongitude));

        var response = await client.PutAsync($"/api/v1/areas/{cell}", null);
        var state = await response.Content.ReadFromJsonAsync<AreaBody>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(state!.Region, Is.EqualTo("vilnius"));
        });
    }

    [Test]
    public async Task FailedLoadAsksTheClientToWait()
    {
        await using var factory = new FactoryWith(new BrokenSource());
        using var client = factory.CreateClient();
        var cell = AreaCell.ForCoordinate(GeoCoordinate.Create(56.9496, 24.1052));

        await client.PutAsync($"/api/v1/areas/{cell}", null);
        await factory.Services.GetRequiredService<BackgroundAreaLoader>().WaitAsync(cell);
        var state = await client.GetFromJsonAsync<AreaBody>($"/api/v1/areas/{cell}");
        var retry = await client.PutAsync($"/api/v1/areas/{cell}", null);

        Assert.Multiple(() =>
        {
            Assert.That(state!.Status, Is.EqualTo("Failed"));
            Assert.That(retry.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(retry.Headers.RetryAfter, Is.Not.Null);
        });
    }

    [Test]
    public async Task WrongResolutionIsRejected()
    {
        await using var factory = new FactoryWith(new BrokenSource());
        using var client = factory.CreateClient();

        var response = await client.PutAsync("/api/v1/areas/8b1f4030c610fff", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ApiLoginMayAddPlacesButNotRemoveThem()
    {
        await using var connection = new NpgsqlConnection(PostgisEnvironment.ApiConnectionString);
        await connection.OpenAsync();
        await using var delete = new NpgsqlCommand("DELETE FROM region WHERE code LIKE 'area-%'", connection);

        var error = Assert.ThrowsAsync<PostgresException>(() => delete.ExecuteNonQueryAsync());

        Assert.That(error!.SqlState, Is.EqualTo(PostgresErrorCodes.InsufficientPrivilege));
    }

    private sealed record AreaBody(string Cell, string Status, string? Region);

    private sealed record PlaceBody(string ExternalId, string Name);
}

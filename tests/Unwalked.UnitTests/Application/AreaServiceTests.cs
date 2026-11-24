using H3;
using Unwalked.Application.Areas;
using Unwalked.Application.Errors;
using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;
using Unwalked.UnitTests.Fakes;

namespace Unwalked.UnitTests.Application;

public class AreaServiceTests
{
    private static readonly string Kaunas = AreaCell.ForCoordinate(GeoCoordinate.Create(54.8985, 23.9036)).ToString();

    private FakeAreaRepository repository = null!;
    private FakeAreaLoader loader = null!;
    private AreaService service = null!;

    [SetUp]
    public void Create()
    {
        repository = new FakeAreaRepository();
        loader = new FakeAreaLoader();
        service = new AreaService(repository, loader, new FixedClock());
    }

    [Test]
    public async Task CoveredAreaIsReadyAndStartsNothing()
    {
        repository.CoveringRegion = "vilnius";

        var state = await service.EnsureAsync(Kaunas, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(state.Status, Is.EqualTo(AreaStatus.Ready));
            Assert.That(state.Region, Is.EqualTo("vilnius"));
            Assert.That(loader.Started, Is.Empty);
        });
    }

    [Test]
    public async Task MissingAreaStartsLoadingOnce()
    {
        var first = await service.EnsureAsync(Kaunas, CancellationToken.None);
        var second = await service.EnsureAsync(Kaunas, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first.Status, Is.EqualTo(AreaStatus.Loading));
            Assert.That(second.Status, Is.EqualTo(AreaStatus.Loading));
            Assert.That(loader.Started, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task GetDoesNotStartALoad()
    {
        var state = await service.GetAsync(Kaunas, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(state.Status, Is.EqualTo(AreaStatus.Missing));
            Assert.That(loader.Started, Is.Empty);
        });
    }

    [Test]
    public void RecentFailureAsksToWait()
    {
        loader.Failures[(ulong)new H3Index(Kaunas)] = FixedClock.Default.AddMinutes(-2);

        var error = Assert.ThrowsAsync<TooManyRequestsException>(() => service.EnsureAsync(Kaunas, CancellationToken.None));

        Assert.That(error!.RetryAfter, Is.EqualTo(TimeSpan.FromMinutes(8)));
    }

    [Test]
    public async Task OldFailureIsRetried()
    {
        loader.Failures[(ulong)new H3Index(Kaunas)] = FixedClock.Default.AddMinutes(-30);

        var state = await service.EnsureAsync(Kaunas, CancellationToken.None);

        Assert.That(state.Status, Is.EqualTo(AreaStatus.Loading));
    }

    [Test]
    public void HourlyLimitStopsNewLoads()
    {
        loader.RecentStarts = AreaService.MaxLoadsPerHour;

        Assert.ThrowsAsync<TooManyRequestsException>(() => service.EnsureAsync(Kaunas, CancellationToken.None));
        Assert.That(loader.Started, Is.Empty);
    }

    [Test]
    public void FullCatalogueStopsNewLoads()
    {
        repository.LoadedAreas = AreaService.MaxAreas;

        Assert.ThrowsAsync<TooManyRequestsException>(() => service.EnsureAsync(Kaunas, CancellationToken.None));
    }

    [TestCase("not-a-cell")]
    [TestCase("8b1f4030c610fff")]
    public void OnlyResolutionFiveCellsAreAreas(string cell)
    {
        Assert.ThrowsAsync<DomainValidationException>(() => service.GetAsync(cell, CancellationToken.None));
    }
}

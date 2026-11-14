using Unwalked.Application.Places;
using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;
using Unwalked.UnitTests.Fakes;

namespace Unwalked.UnitTests.Application;

public class PlaceQueryServiceTests
{
    private static readonly GeoCoordinate OldTown = GeoCoordinate.Create(54.6810, 25.2870);

    private RecordingPlaceReadRepository places = null!;
    private PlaceQueryService service = null!;

    [SetUp]
    public void CreateService()
    {
        places = new RecordingPlaceReadRepository();
        service = new PlaceQueryService(places);
    }

    [Test]
    public async Task FindNear_PassesValidatedArgumentsToRepository()
    {
        await service.FindNearAsync(OldTown.Latitude, OldTown.Longitude, 1000, " CAFE ", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(places.LastCenter, Is.EqualTo(OldTown));
            Assert.That(places.LastRadius, Is.EqualTo(1000));
            Assert.That(places.LastCategory, Is.EqualTo("cafe"));
            Assert.That(places.LastLimit, Is.EqualTo(ResultLimit.Default));
        });
    }

    [Test]
    public void FindNear_RejectsRadiusBeforeQuerying()
    {
        Assert.Throws<DomainValidationException>(() => service.FindNearAsync(OldTown.Latitude, OldTown.Longitude, 9000, null, null, CancellationToken.None));
        Assert.That(places.LastCenter, Is.Null);
    }

    [Test]
    public async Task FindInBoundingBox_PassesBoxAndLimit()
    {
        await service.FindInBoundingBoxAsync(54.67, 25.25, 54.70, 25.30, null, 10, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(places.LastBox, Is.EqualTo(BoundingBox.Create(54.67, 25.25, 54.70, 25.30)));
            Assert.That(places.LastCategory, Is.Null);
            Assert.That(places.LastLimit, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task FindInCells_ExpandsCellsToPlaceResolution()
    {
        var parent = ParentText(9);

        await service.FindInCellsAsync([parent], null, 200, CancellationToken.None);

        Assert.That(places.LastCells, Has.Count.EqualTo(49));
    }

    [Test]
    public void ExpandCells_DeduplicatesOverlappingCells()
    {
        var parent = ParentText(10);
        var child = H3Cell.Format(H3Cell.ForCoordinate(OldTown));

        var expanded = PlaceQueryService.ExpandCells([parent, child, parent]);

        Assert.That(expanded, Has.Count.EqualTo(7));
    }

    [Test]
    public void ExpandCells_RejectsEmptyList()
    {
        Assert.Throws<DomainValidationException>(() => PlaceQueryService.ExpandCells([]));
    }

    [Test]
    public void ExpandCells_RejectsTooManyRequestedCells()
    {
        var cells = Enumerable.Repeat(H3Cell.Format(H3Cell.ForCoordinate(OldTown)), PlaceQueryService.MaxRequestedCells + 1).ToList();

        Assert.Throws<DomainValidationException>(() => PlaceQueryService.ExpandCells(cells));
    }

    [Test]
    public void ExpandCells_RejectsCellsCoveringTooLargeAnArea()
    {
        var resolution7Cells = Enumerable.Range(0, 9)
            .Select(offset => ParentText(7, GeoCoordinate.Create(54.0 + offset * 0.2, 25.0)))
            .Distinct()
            .ToList();

        var error = Assert.Throws<DomainValidationException>(() => PlaceQueryService.ExpandCells(resolution7Cells));

        Assert.That(error!.Message, Does.Contain(PlaceQueryService.MaxExpandedCells.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static string ParentText(int resolution, GeoCoordinate? at = null)
    {
        var point = at ?? OldTown;
        var latLng = H3.Model.LatLng.FromCoordinate(new NetTopologySuite.Geometries.Coordinate(point.Longitude, point.Latitude));
        return H3.H3Index.FromLatLng(latLng, resolution).ToString();
    }
}

using H3;
using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;

namespace Unwalked.UnitTests.Domain;

public class H3CellTests
{
    private static readonly GeoCoordinate VilniusCathedral = GeoCoordinate.Create(54.6858, 25.2877);

    [Test]
    public void ForCoordinate_ReturnsResolution11CellThatFormatsAndParsesBack()
    {
        var cell = H3Cell.ForCoordinate(VilniusCathedral);

        var parsed = H3Cell.Parse(H3Cell.Format(cell));

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Resolution, Is.EqualTo(H3Cell.PlaceResolution));
            Assert.That((long)(ulong)parsed, Is.EqualTo(cell));
        });
    }

    [Test]
    public void ForCoordinate_GivesDifferentCellsForPointsFarApart()
    {
        var other = GeoCoordinate.Create(54.7000, 25.3000);

        Assert.That(H3Cell.ForCoordinate(other), Is.Not.EqualTo(H3Cell.ForCoordinate(VilniusCathedral)));
    }

    [TestCase("nope")]
    [TestCase("")]
    [TestCase("ffffffffffffffff")]
    [TestCase("0")]
    public void Parse_RejectsTextThatIsNotACell(string text)
    {
        var error = Assert.Throws<DomainValidationException>(() => H3Cell.Parse(text));

        Assert.That(error!.Field, Is.EqualTo("cells"));
    }

    [Test]
    public void Parse_TrimsWhitespace()
    {
        var text = H3Cell.Format(H3Cell.ForCoordinate(VilniusCathedral));

        Assert.That(H3Cell.Parse($"  {text} ").ToString(), Is.EqualTo(text));
    }

    [TestCase(11, 1)]
    [TestCase(10, 7)]
    [TestCase(9, 49)]
    [TestCase(7, 2401)]
    public void ExpandToPlaceResolution_ReturnsAllResolution11Descendants(int resolution, int expectedCount)
    {
        var cell = ParentAt(resolution);

        var children = H3Cell.ExpandToPlaceResolution(cell).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(children, Has.Count.EqualTo(expectedCount));
            Assert.That(children, Is.Unique);
            Assert.That(children.Select(child => new H3Index((ulong)child).Resolution), Is.All.EqualTo(H3Cell.PlaceResolution));
        });
    }

    [TestCase(6)]
    [TestCase(12)]
    public void ExpandToPlaceResolution_RejectsResolutionOutsideQueryRange(int resolution)
    {
        var cell = ParentAt(resolution);

        Assert.Throws<DomainValidationException>(() => H3Cell.ExpandToPlaceResolution(cell).ToList());
    }

    private static H3Index ParentAt(int resolution)
    {
        var latLng = H3.Model.LatLng.FromCoordinate(new NetTopologySuite.Geometries.Coordinate(VilniusCathedral.Longitude, VilniusCathedral.Latitude));
        return H3Index.FromLatLng(latLng, resolution);
    }
}

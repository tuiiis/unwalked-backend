using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;

namespace Unwalked.UnitTests.Domain;

public class BoundingBoxTests
{
    [Test]
    public void Create_KeepsCorners()
    {
        var box = BoundingBox.Create(54.67, 25.25, 54.70, 25.30);

        Assert.Multiple(() =>
        {
            Assert.That(box.SouthWest, Is.EqualTo(GeoCoordinate.Create(54.67, 25.25)));
            Assert.That(box.NorthEast, Is.EqualTo(GeoCoordinate.Create(54.70, 25.30)));
        });
    }

    [TestCase(54.70, 25.25, 54.67, 25.30)]
    [TestCase(54.67, 25.30, 54.70, 25.25)]
    [TestCase(54.67, 25.25, 54.67, 25.30)]
    public void Create_RejectsInvertedOrEmptyBox(double minLat, double minLon, double maxLat, double maxLon)
    {
        var error = Assert.Throws<DomainValidationException>(() => BoundingBox.Create(minLat, minLon, maxLat, maxLon));

        Assert.That(error!.Field, Is.EqualTo("bbox"));
    }

    [Test]
    public void Create_RejectsBoxLargerThanLimit()
    {
        var error = Assert.Throws<DomainValidationException>(() => BoundingBox.Create(54.0, 25.0, 54.6, 25.6));

        Assert.That(error!.Message, Does.Contain(BoundingBox.MaxAreaSquareDegrees.ToString(System.Globalization.CultureInfo.CurrentCulture)));
    }

    [Test]
    public void Create_RejectsInvalidCorner()
    {
        var error = Assert.Throws<DomainValidationException>(() => BoundingBox.Create(54.0, 25.0, 95.0, 25.1));

        Assert.That(error!.Field, Is.EqualTo("lat"));
    }
}

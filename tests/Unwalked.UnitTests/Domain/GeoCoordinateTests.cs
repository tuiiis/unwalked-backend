using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;

namespace Unwalked.UnitTests.Domain;

public class GeoCoordinateTests
{
    [TestCase(54.6872, 25.2797)]
    [TestCase(-90, -180)]
    [TestCase(90, 180)]
    public void Create_AcceptsCoordinatesInRange(double latitude, double longitude)
    {
        var coordinate = GeoCoordinate.Create(latitude, longitude);

        Assert.Multiple(() =>
        {
            Assert.That(coordinate.Latitude, Is.EqualTo(latitude));
            Assert.That(coordinate.Longitude, Is.EqualTo(longitude));
        });
    }

    [TestCase(90.0001, 0, "lat")]
    [TestCase(-91, 0, "lat")]
    [TestCase(double.NaN, 0, "lat")]
    [TestCase(0, 180.5, "lon")]
    [TestCase(0, -181, "lon")]
    [TestCase(0, double.NaN, "lon")]
    public void Create_RejectsCoordinatesOutOfRange(double latitude, double longitude, string field)
    {
        var error = Assert.Throws<DomainValidationException>(() => GeoCoordinate.Create(latitude, longitude));

        Assert.That(error!.Field, Is.EqualTo(field));
    }
}

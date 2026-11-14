using Unwalked.Domain.Common;
using Unwalked.Domain.Places;

namespace Unwalked.UnitTests.Domain;

public class PlaceRulesTests
{
    [TestCase(" Cafe ", "cafe")]
    [TestCase("coffee_shop", "coffee_shop")]
    [TestCase("MUSEUM2", "museum2")]
    public void CategoryCode_NormalizesToLowerCase(string input, string expected)
    {
        Assert.That(CategoryCode.Normalize(input), Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("coffee shop")]
    [TestCase("café")]
    [TestCase("drop;table")]
    public void CategoryCode_RejectsCodesOutsideAllowedAlphabet(string input)
    {
        var error = Assert.Throws<DomainValidationException>(() => CategoryCode.Normalize(input));

        Assert.That(error!.Field, Is.EqualTo("category"));
    }

    [Test]
    public void CategoryCode_RejectsCodesLongerThanLimit()
    {
        Assert.Throws<DomainValidationException>(() => CategoryCode.Normalize(new string('a', CategoryCode.MaxLength + 1)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void CategoryCode_TreatsBlankOptionalCodeAsNoFilter(string? input)
    {
        Assert.That(CategoryCode.NormalizeOptional(input), Is.Null);
    }

    [TestCase(SearchRadius.MinMeters)]
    [TestCase(1000)]
    [TestCase(SearchRadius.MaxMeters)]
    public void SearchRadius_AcceptsValuesInRange(double meters)
    {
        Assert.That(SearchRadius.Validate(meters), Is.EqualTo(meters));
    }

    [TestCase(9.99)]
    [TestCase(5000.01)]
    [TestCase(double.NaN)]
    public void SearchRadius_RejectsValuesOutOfRange(double meters)
    {
        var error = Assert.Throws<DomainValidationException>(() => SearchRadius.Validate(meters));

        Assert.That(error!.Field, Is.EqualTo("radius"));
    }

    [TestCase(null, ResultLimit.Default)]
    [TestCase(1, 1)]
    [TestCase(ResultLimit.Max, ResultLimit.Max)]
    public void ResultLimit_ResolvesRequestedOrDefault(int? requested, int expected)
    {
        Assert.That(ResultLimit.Resolve(requested), Is.EqualTo(expected));
    }

    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(ResultLimit.Max + 1)]
    public void ResultLimit_RejectsValuesOutOfRange(int requested)
    {
        var error = Assert.Throws<DomainValidationException>(() => ResultLimit.Resolve(requested));

        Assert.That(error!.Field, Is.EqualTo("limit"));
    }
}

using Unwalked.Domain.Common;
using Unwalked.Domain.Users;

namespace Unwalked.UnitTests.Domain;

public class UserRulesTests
{
    [TestCase(" Tanya@Example.LT ", "tanya@example.lt")]
    [TestCase("walker+vilnius@example.com", "walker+vilnius@example.com")]
    public void EmailAddress_TrimsAndLowerCases(string input, string expected)
    {
        Assert.That(EmailAddress.Normalize(input), Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("not-an-email")]
    [TestCase("Tanya <tanya@example.lt>")]
    [TestCase("a@b@c")]
    public void EmailAddress_RejectsInvalidAddresses(string input)
    {
        var error = Assert.Throws<DomainValidationException>(() => EmailAddress.Normalize(input));

        Assert.That(error!.Field, Is.EqualTo("email"));
    }

    [Test]
    public void EmailAddress_RejectsAddressesLongerThanLimit()
    {
        var tooLong = new string('a', EmailAddress.MaxLength) + "@example.lt";

        Assert.Throws<DomainValidationException>(() => EmailAddress.Normalize(tooLong));
    }

    [Test]
    public void PasswordPolicy_AcceptsPasswordWithinLengthLimits()
    {
        Assert.DoesNotThrow(() => PasswordPolicy.Validate("correct horse battery"));
    }

    [TestCase(PasswordPolicy.MinLength - 1)]
    [TestCase(PasswordPolicy.MaxLength + 1)]
    public void PasswordPolicy_RejectsPasswordOutsideLengthLimits(int length)
    {
        var error = Assert.Throws<DomainValidationException>(() => PasswordPolicy.Validate(new string('x', length)));

        Assert.That(error!.Field, Is.EqualTo("password"));
    }

    [Test]
    public void PasswordPolicy_RejectsBlankPassword()
    {
        var error = Assert.Throws<DomainValidationException>(() => PasswordPolicy.Validate(new string(' ', PasswordPolicy.MinLength)));

        Assert.That(error!.Message, Does.Contain("blank"));
    }
}

using Unwalked.Application.Errors;
using Unwalked.Application.Sharing;
using Unwalked.Domain.Common;
using Unwalked.Domain.Sharing;
using Unwalked.UnitTests.Domain;
using Unwalked.UnitTests.Fakes;

namespace Unwalked.UnitTests.Application;

public class ShareServiceTests
{
    private static readonly Guid Owner = Guid.Parse("0b0b0b0b-0000-4000-8000-000000000001");
    private static readonly Guid Stranger = Guid.Parse("0b0b0b0b-0000-4000-8000-000000000002");
    private static readonly DateOnly Today = DateOnly.FromDateTime(FixedClock.Default.UtcDateTime);

    private InMemoryShareRepository shares = null!;
    private ShareService service = null!;

    [SetUp]
    public void CreateService()
    {
        shares = new InMemoryShareRepository();
        service = new ShareService(shares, new FixedClock());
    }

    [Test]
    public async Task Create_StoresTrimmedShareWithDecodedImage()
    {
        var details = await service.CreateAsync(Owner, ValidCommand("  50 new cells  "), CancellationToken.None);

        var stored = shares.Shares[details.Id];
        Assert.Multiple(() =>
        {
            Assert.That(details, Is.EqualTo(new ShareDetails(details.Id, "50 new cells", Today, ShareImage.Png, FixedClock.Default)));
            Assert.That(stored.OwnerId, Is.EqualTo(Owner));
            Assert.That(stored.Image, Is.EqualTo(ShareImageTests.TinyPng));
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_RejectsBlankName(string name)
    {
        var error = Assert.ThrowsAsync<DomainValidationException>(() => service.CreateAsync(Owner, ValidCommand(name), CancellationToken.None));

        Assert.That(error!.Field, Is.EqualTo("achievementName"));
    }

    [Test]
    public void Create_RejectsNameLongerThanLimit()
    {
        var command = ValidCommand(new string('n', AchievementShare.MaxNameLength + 1));

        Assert.ThrowsAsync<DomainValidationException>(() => service.CreateAsync(Owner, command, CancellationToken.None));
    }

    [Test]
    public void Create_RejectsDateMoreThanADayAhead()
    {
        var command = ValidCommand("Future walk") with { AchievedOn = Today.AddDays(2) };

        var error = Assert.ThrowsAsync<DomainValidationException>(() => service.CreateAsync(Owner, command, CancellationToken.None));

        Assert.That(error!.Field, Is.EqualTo("achievedOn"));
    }

    [Test]
    public async Task Create_AllowsTomorrowForClientsAheadOfUtc()
    {
        var command = ValidCommand("Late night walk") with { AchievedOn = Today.AddDays(1) };

        var details = await service.CreateAsync(Owner, command, CancellationToken.None);

        Assert.That(details.AchievedOn, Is.EqualTo(Today.AddDays(1)));
    }

    [Test]
    public void Create_RejectsImageNotMatchingType()
    {
        var command = ValidCommand("Old town") with { ImageContentType = ShareImage.Jpeg };

        Assert.ThrowsAsync<DomainValidationException>(() => service.CreateAsync(Owner, command, CancellationToken.None));
        Assert.That(shares.Shares, Is.Empty);
    }

    [Test]
    public async Task GetAndGetImage_ReturnStoredShare()
    {
        var created = await service.CreateAsync(Owner, ValidCommand("Old town"), CancellationToken.None);

        var details = await service.GetAsync(created.Id, CancellationToken.None);
        var image = await service.GetImageAsync(created.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(details, Is.EqualTo(created));
            Assert.That(image.Content, Is.EqualTo(ShareImageTests.TinyPng));
        });
    }

    [Test]
    public void GetAndGetImage_ThrowNotFoundForUnknownShare()
    {
        var unknown = Guid.NewGuid();

        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(unknown, CancellationToken.None));
            Assert.ThrowsAsync<NotFoundException>(() => service.GetImageAsync(unknown, CancellationToken.None));
        });
    }

    [Test]
    public async Task Delete_RemovesShareForOwner()
    {
        var created = await service.CreateAsync(Owner, ValidCommand("Old town"), CancellationToken.None);

        await service.DeleteAsync(Owner, created.Id, CancellationToken.None);

        Assert.That(shares.Shares, Is.Empty);
    }

    [Test]
    public async Task Delete_HidesOtherOwnersSharesAsNotFound()
    {
        var created = await service.CreateAsync(Owner, ValidCommand("Old town"), CancellationToken.None);

        Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Stranger, created.Id, CancellationToken.None));
        Assert.That(shares.Shares, Has.Count.EqualTo(1));
    }

    private static CreateShareCommand ValidCommand(string name) =>
        new(name, Today, ShareImage.Png, Convert.ToBase64String(ShareImageTests.TinyPng));
}

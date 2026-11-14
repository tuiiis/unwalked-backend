using Unwalked.Application.Feedback;
using Unwalked.Domain.Common;
using Unwalked.Domain.Feedback;
using Unwalked.UnitTests.Fakes;

namespace Unwalked.UnitTests.Application;

public class FeedbackServiceTests
{
    private const int CafeCategoryId = 7;
    private static readonly DateOnly Today = DateOnly.FromDateTime(FixedClock.Default.UtcDateTime);

    private InMemoryFeedbackRepository feedback = null!;
    private FeedbackService service = null!;

    [SetUp]
    public void CreateService()
    {
        feedback = new InMemoryFeedbackRepository();
        var categories = new DictionaryCategoryRepository(new Dictionary<string, int> { ["cafe"] = CafeCategoryId });
        service = new FeedbackService(feedback, categories, new FixedClock());
    }

    [Test]
    public async Task Submit_StoresParsedFeedbackWithDayOnlyTimestamp()
    {
        var eventId = Guid.NewGuid();

        var added = await service.SubmitAsync(new SubmitFeedbackCommand(eventId, "nearby_new_place", "accepted", " Cafe "), CancellationToken.None);

        var stored = feedback.Entries.Single();
        Assert.Multiple(() =>
        {
            Assert.That(added, Is.True);
            Assert.That(stored.EventId, Is.EqualTo(eventId));
            Assert.That(stored.RecommendationType, Is.EqualTo(RecommendationType.NearbyNewPlace));
            Assert.That(stored.Action, Is.EqualTo(FeedbackAction.Accepted));
            Assert.That(stored.CategoryId, Is.EqualTo(CafeCategoryId));
            Assert.That(stored.ReceivedOn, Is.EqualTo(Today));
        });
    }

    [Test]
    public async Task Submit_IsIdempotentByEventId()
    {
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), "walking_route", "dismissed", null);

        var first = await service.SubmitAsync(command, CancellationToken.None);
        var second = await service.SubmitAsync(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(feedback.Entries, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Submit_RejectsEmptyEventId()
    {
        var error = Assert.ThrowsAsync<DomainValidationException>(() =>
            service.SubmitAsync(new SubmitFeedbackCommand(Guid.Empty, "walking_route", "visited", null), CancellationToken.None));

        Assert.That(error!.Field, Is.EqualTo("eventId"));
    }

    [TestCase("teleport", "accepted", "recommendationType")]
    [TestCase("walking_route", "liked", "action")]
    public void Submit_RejectsUnknownWireNames(string type, string action, string field)
    {
        var error = Assert.ThrowsAsync<DomainValidationException>(() =>
            service.SubmitAsync(new SubmitFeedbackCommand(Guid.NewGuid(), type, action, null), CancellationToken.None));

        Assert.That(error!.Field, Is.EqualTo(field));
    }

    [Test]
    public void Submit_RejectsUnknownCategory()
    {
        var error = Assert.ThrowsAsync<DomainValidationException>(() =>
            service.SubmitAsync(new SubmitFeedbackCommand(Guid.NewGuid(), "walking_route", "visited", "casino"), CancellationToken.None));

        Assert.That(error!.Message, Is.EqualTo("Unknown category 'casino'."));
    }

    [Test]
    public async Task Stats_DefaultsToLastThirtyDays()
    {
        await service.GetStatsAsync(null, null, CancellationToken.None);

        Assert.That(feedback.LastCountedRange, Is.EqualTo((Today.AddDays(-30), Today)));
    }

    [Test]
    public async Task Stats_SummarizesStoredFeedback()
    {
        await service.SubmitAsync(new SubmitFeedbackCommand(Guid.NewGuid(), "unvisited_cell", "accepted", null), CancellationToken.None);
        await service.SubmitAsync(new SubmitFeedbackCommand(Guid.NewGuid(), "unvisited_cell", "dismissed", null), CancellationToken.None);

        var stats = await service.GetStatsAsync(Today, Today, CancellationToken.None);

        Assert.That(stats.Single(row => row.RecommendationType == RecommendationType.UnvisitedCell).AcceptanceRate, Is.EqualTo(0.5));
    }

    [Test]
    public void Stats_RejectsInvertedRange()
    {
        var error = Assert.ThrowsAsync<DomainValidationException>(() => service.GetStatsAsync(Today, Today.AddDays(-1), CancellationToken.None));

        Assert.That(error!.Field, Is.EqualTo("from"));
    }

    [Test]
    public void Stats_RejectsRangeLongerThanLimit()
    {
        var from = Today.AddDays(-(FeedbackService.MaxStatsRangeDays + 1));

        var error = Assert.ThrowsAsync<DomainValidationException>(() => service.GetStatsAsync(from, Today, CancellationToken.None));

        Assert.That(error!.Field, Is.EqualTo("to"));
    }
}

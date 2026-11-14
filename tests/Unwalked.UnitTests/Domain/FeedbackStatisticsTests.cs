using Unwalked.Domain.Feedback;

namespace Unwalked.UnitTests.Domain;

public class FeedbackStatisticsTests
{
    [Test]
    public void Summarize_ReturnsEveryRecommendationTypeEvenWithoutFeedback()
    {
        var stats = FeedbackStatistics.Summarize([]);

        Assert.Multiple(() =>
        {
            Assert.That(stats.Select(row => row.RecommendationType), Is.EqualTo(Enum.GetValues<RecommendationType>()));
            Assert.That(stats.Select(row => row.AcceptanceRate), Is.All.Null);
            Assert.That(stats.Select(row => row.VisitRate), Is.All.Null);
        });
    }

    [Test]
    public void Summarize_ComputesAcceptanceAndVisitRates()
    {
        FeedbackCount[] counts =
        [
            new(RecommendationType.NearbyNewPlace, FeedbackAction.Accepted, 2),
            new(RecommendationType.NearbyNewPlace, FeedbackAction.Accepted, 1),
            new(RecommendationType.NearbyNewPlace, FeedbackAction.Dismissed, 6),
            new(RecommendationType.NearbyNewPlace, FeedbackAction.Visited, 1),
            new(RecommendationType.WalkingRoute, FeedbackAction.Dismissed, 4),
        ];

        var stats = FeedbackStatistics.Summarize(counts).ToDictionary(row => row.RecommendationType);

        Assert.Multiple(() =>
        {
            Assert.That(stats[RecommendationType.NearbyNewPlace], Is.EqualTo(new RecommendationTypeStats(RecommendationType.NearbyNewPlace, 3, 6, 1, 0.3333, 0.3333)));
            Assert.That(stats[RecommendationType.WalkingRoute], Is.EqualTo(new RecommendationTypeStats(RecommendationType.WalkingRoute, 0, 4, 0, 0, null)));
            Assert.That(stats[RecommendationType.UnvisitedCell].AcceptanceRate, Is.Null);
        });
    }
}

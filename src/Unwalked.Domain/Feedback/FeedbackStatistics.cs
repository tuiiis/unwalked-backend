namespace Unwalked.Domain.Feedback;

public sealed record FeedbackCount(RecommendationType RecommendationType, FeedbackAction Action, long Count);

public sealed record RecommendationTypeStats(
    RecommendationType RecommendationType,
    long Accepted,
    long Dismissed,
    long Visited,
    double? AcceptanceRate,
    double? VisitRate);

public static class FeedbackStatistics
{
    public static IReadOnlyList<RecommendationTypeStats> Summarize(IEnumerable<FeedbackCount> counts)
    {
        var byType = counts
            .GroupBy(count => count.RecommendationType)
            .ToDictionary(group => group.Key, group => group.ToList());

        return Enum.GetValues<RecommendationType>()
            .Select(type => Summarize(type, byType.GetValueOrDefault(type) ?? []))
            .ToList();
    }

    private static RecommendationTypeStats Summarize(RecommendationType type, IReadOnlyCollection<FeedbackCount> counts)
    {
        var accepted = Sum(counts, FeedbackAction.Accepted);
        var dismissed = Sum(counts, FeedbackAction.Dismissed);
        var visited = Sum(counts, FeedbackAction.Visited);
        var decided = accepted + dismissed;

        return new RecommendationTypeStats(
            type,
            accepted,
            dismissed,
            visited,
            Ratio(accepted, decided),
            Ratio(visited, accepted));
    }

    private static long Sum(IEnumerable<FeedbackCount> counts, FeedbackAction action) =>
        counts.Where(count => count.Action == action).Sum(count => count.Count);

    private static double? Ratio(long numerator, long denominator) =>
        denominator == 0 ? null : Math.Round((double)numerator / denominator, 4);
}

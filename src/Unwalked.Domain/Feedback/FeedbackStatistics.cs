namespace Unwalked.Domain.Feedback;

/// <summary>
/// Number of feedback events of one type and action.
/// </summary>
public sealed record FeedbackCount(RecommendationType RecommendationType, FeedbackAction Action, long Count);

/// <summary>
/// Counts and rates for one recommendation type. A rate is null when its denominator is zero.
/// </summary>
public sealed record RecommendationTypeStats(
    RecommendationType RecommendationType,
    long Accepted,
    long Dismissed,
    long Visited,
    double? AcceptanceRate,
    double? VisitRate);

/// <summary>
/// Turns raw feedback counts into the rates used in the study.
/// </summary>
public static class FeedbackStatistics
{
    /// <summary>
    /// Returns one entry per recommendation type, including types without feedback. Acceptance rate is accepted over accepted plus dismissed, visit rate is visited over accepted, both rounded to four places.
    /// </summary>
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

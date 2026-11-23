using System.ComponentModel.DataAnnotations;
using Unwalked.Domain.Feedback;

namespace Unwalked.Api.Contracts;

/// <summary>
/// One anonymous reaction to a recommendation. The client generates the event id so retries can be recognised.
/// </summary>
public sealed record SubmitFeedbackRequest(
    [Required] Guid EventId,
    [Required] string RecommendationType,
    [Required] string Action,
    string? Category);

/// <summary>
/// Feedback totals per recommendation type for the requested date range.
/// </summary>
public sealed record FeedbackStatsResponse(DateOnly? From, DateOnly? To, IReadOnlyList<RecommendationTypeStatsResponse> Types);

/// <summary>
/// Counts and rates for one recommendation type.
/// </summary>
public sealed record RecommendationTypeStatsResponse(
    string RecommendationType,
    long Accepted,
    long Dismissed,
    long Visited,
    double? AcceptanceRate,
    double? VisitRate)
{
    /// <summary>
    /// Maps domain statistics to the wire format.
    /// </summary>
    public static RecommendationTypeStatsResponse From(RecommendationTypeStats stats) => new(
        WireName.From(stats.RecommendationType),
        stats.Accepted,
        stats.Dismissed,
        stats.Visited,
        stats.AcceptanceRate,
        stats.VisitRate);
}

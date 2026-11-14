using System.ComponentModel.DataAnnotations;
using Unwalked.Domain.Feedback;

namespace Unwalked.Api.Contracts;

public sealed record SubmitFeedbackRequest(
    [Required] Guid EventId,
    [Required] string RecommendationType,
    [Required] string Action,
    string? Category);

public sealed record FeedbackStatsResponse(DateOnly? From, DateOnly? To, IReadOnlyList<RecommendationTypeStatsResponse> Types);

public sealed record RecommendationTypeStatsResponse(
    string RecommendationType,
    long Accepted,
    long Dismissed,
    long Visited,
    double? AcceptanceRate,
    double? VisitRate)
{
    public static RecommendationTypeStatsResponse From(RecommendationTypeStats stats) => new(
        WireName.From(stats.RecommendationType),
        stats.Accepted,
        stats.Dismissed,
        stats.Visited,
        stats.AcceptanceRate,
        stats.VisitRate);
}

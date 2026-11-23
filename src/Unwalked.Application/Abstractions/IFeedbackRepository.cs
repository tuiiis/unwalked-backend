using Unwalked.Domain.Feedback;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Storage of anonymous recommendation feedback.
/// </summary>
public interface IFeedbackRepository
{
    /// <summary>
    /// Stores the feedback unless an entry with the same event id exists. Returns true when it was stored.
    /// </summary>
    Task<bool> AddIfNewAsync(RecommendationFeedback feedback, CancellationToken cancellationToken);

    /// <summary>
    /// Counts feedback per recommendation type and action for the inclusive date range.
    /// </summary>
    Task<IReadOnlyList<FeedbackCount>> CountAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
}

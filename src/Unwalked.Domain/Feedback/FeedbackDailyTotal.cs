namespace Unwalked.Domain.Feedback;

/// <summary>
/// Feedback events per day, type, action and category, kept so statistics outlive the raw events.
/// </summary>
public sealed class FeedbackDailyTotal
{
    public long Id { get; set; }

    public DateOnly ReceivedOn { get; set; }

    public RecommendationType RecommendationType { get; set; }

    public FeedbackAction Action { get; set; }

    public int? CategoryId { get; set; }

    /// <summary>
    /// Number of events in this group.
    /// </summary>
    public int EventCount { get; set; }
}

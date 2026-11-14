namespace Unwalked.Domain.Feedback;

public sealed class FeedbackDailyTotal
{
    public long Id { get; set; }

    public DateOnly ReceivedOn { get; set; }

    public RecommendationType RecommendationType { get; set; }

    public FeedbackAction Action { get; set; }

    public int? CategoryId { get; set; }

    public int EventCount { get; set; }
}

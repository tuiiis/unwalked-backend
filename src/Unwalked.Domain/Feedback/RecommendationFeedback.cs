namespace Unwalked.Domain.Feedback;

public sealed class RecommendationFeedback
{
    public long Id { get; set; }

    public Guid EventId { get; set; }

    public RecommendationType RecommendationType { get; set; }

    public FeedbackAction Action { get; set; }

    public int? CategoryId { get; set; }

    public DateOnly ReceivedOn { get; set; }
}

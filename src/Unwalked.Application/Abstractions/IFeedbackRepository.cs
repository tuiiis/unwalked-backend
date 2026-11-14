using Unwalked.Domain.Feedback;

namespace Unwalked.Application.Abstractions;

public interface IFeedbackRepository
{
    Task<bool> AddIfNewAsync(RecommendationFeedback feedback, CancellationToken cancellationToken);

    Task<IReadOnlyList<FeedbackCount>> CountAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
}

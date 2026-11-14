using Unwalked.Application.Abstractions;
using Unwalked.Domain.Feedback;

namespace Unwalked.UnitTests.Fakes;

public sealed class InMemoryFeedbackRepository : IFeedbackRepository
{
    public List<RecommendationFeedback> Entries { get; } = [];

    public (DateOnly From, DateOnly To)? LastCountedRange { get; private set; }

    public Task<bool> AddIfNewAsync(RecommendationFeedback feedback, CancellationToken cancellationToken)
    {
        if (Entries.Any(entry => entry.EventId == feedback.EventId))
        {
            return Task.FromResult(false);
        }

        Entries.Add(feedback);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<FeedbackCount>> CountAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        LastCountedRange = (from, to);
        IReadOnlyList<FeedbackCount> counts = Entries
            .Where(entry => entry.ReceivedOn >= from && entry.ReceivedOn <= to)
            .GroupBy(entry => (entry.RecommendationType, entry.Action))
            .Select(group => new FeedbackCount(group.Key.RecommendationType, group.Key.Action, group.LongCount()))
            .ToList();
        return Task.FromResult(counts);
    }
}

using Unwalked.Application.Abstractions;
using Unwalked.Domain.Common;
using Unwalked.Domain.Feedback;
using Unwalked.Domain.Places;

namespace Unwalked.Application.Feedback;

public sealed class FeedbackService(IFeedbackRepository feedback, ICategoryRepository categories, IClock clock)
{
    public const int MaxStatsRangeDays = 366;

    public async Task<bool> SubmitAsync(SubmitFeedbackCommand command, CancellationToken cancellationToken)
    {
        if (command.EventId == Guid.Empty)
        {
            throw new DomainValidationException("eventId", "Event id must be a non-empty UUID.");
        }

        var type = WireName.Parse<RecommendationType>("recommendationType", command.RecommendationType);
        var action = WireName.Parse<FeedbackAction>("action", command.Action);
        var categoryId = await ResolveCategoryAsync(command.Category, cancellationToken);

        var entry = new RecommendationFeedback
        {
            EventId = command.EventId,
            RecommendationType = type,
            Action = action,
            CategoryId = categoryId,
            ReceivedOn = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
        };

        return await feedback.AddIfNewAsync(entry, cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendationTypeStats>> GetStatsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var end = to ?? DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var start = from ?? end.AddDays(-30);
        if (start > end)
        {
            throw new DomainValidationException("from", "'from' must not be after 'to'.");
        }

        if (end.DayNumber - start.DayNumber > MaxStatsRangeDays)
        {
            throw new DomainValidationException("to", $"The range must not exceed {MaxStatsRangeDays} days.");
        }

        var counts = await feedback.CountAsync(start, end, cancellationToken);
        return FeedbackStatistics.Summarize(counts);
    }

    private async Task<int?> ResolveCategoryAsync(string? category, CancellationToken cancellationToken)
    {
        var code = CategoryCode.NormalizeOptional(category);
        if (code is null)
        {
            return null;
        }

        return await categories.FindIdByCodeAsync(code, cancellationToken)
            ?? throw new DomainValidationException("category", $"Unknown category '{code}'.");
    }
}

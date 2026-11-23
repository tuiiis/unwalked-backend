using Microsoft.EntityFrameworkCore;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Feedback;

namespace Unwalked.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of feedback storage. A repeated event id is caught by the unique index.
/// </summary>
public sealed class FeedbackRepository(UnwalkedDbContext db) : IFeedbackRepository
{
    public async Task<bool> AddIfNewAsync(RecommendationFeedback feedback, CancellationToken cancellationToken)
    {
        db.Feedback.Add(feedback);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            db.Entry(feedback).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<IReadOnlyList<FeedbackCount>> CountAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
        await db.Feedback
            .Where(feedback => feedback.ReceivedOn >= from && feedback.ReceivedOn <= to)
            .GroupBy(feedback => new { feedback.RecommendationType, feedback.Action })
            .Select(group => new FeedbackCount(group.Key.RecommendationType, group.Key.Action, group.LongCount()))
            .ToListAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Sharing;

namespace Unwalked.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of share storage. A share for a deleted owner is caught by the foreign key.
/// </summary>
public sealed class ShareRepository(UnwalkedDbContext db) : IShareRepository
{
    public async Task<bool> TryAddAsync(AchievementShare share, CancellationToken cancellationToken)
    {
        db.Shares.Add(share);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsForeignKeyViolation(exception))
        {
            db.Entry(share).State = EntityState.Detached;
            return false;
        }
    }

    public Task<AchievementShare?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Shares.AsNoTracking().SingleOrDefaultAsync(share => share.Id == id, cancellationToken);

    public async Task<bool> DeleteOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken) =>
        await db.Shares
            .Where(share => share.Id == id && share.OwnerId == ownerId)
            .ExecuteDeleteAsync(cancellationToken) > 0;
}

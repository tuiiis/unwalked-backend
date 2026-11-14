using Microsoft.EntityFrameworkCore;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Sharing;

namespace Unwalked.Infrastructure.Persistence.Repositories;

public sealed class ShareRepository(UnwalkedDbContext db) : IShareRepository
{
    public async Task AddAsync(AchievementShare share, CancellationToken cancellationToken)
    {
        db.Shares.Add(share);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<AchievementShare?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Shares.AsNoTracking().SingleOrDefaultAsync(share => share.Id == id, cancellationToken);

    public async Task<bool> DeleteOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken) =>
        await db.Shares
            .Where(share => share.Id == id && share.OwnerId == ownerId)
            .ExecuteDeleteAsync(cancellationToken) > 0;
}

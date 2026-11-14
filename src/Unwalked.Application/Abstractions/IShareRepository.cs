using Unwalked.Domain.Sharing;

namespace Unwalked.Application.Abstractions;

public interface IShareRepository
{
    Task AddAsync(AchievementShare share, CancellationToken cancellationToken);

    Task<AchievementShare?> FindAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> DeleteOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);
}

using Unwalked.Domain.Sharing;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Storage of shared achievements.
/// </summary>
public interface IShareRepository
{
    /// <summary>
    /// Stores a new share. Returns false when the owner account no longer exists.
    /// </summary>
    Task<bool> TryAddAsync(AchievementShare share, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the share with this id, or null.
    /// </summary>
    Task<AchievementShare?> FindAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the share only if it belongs to the owner. Returns false when nothing was deleted.
    /// </summary>
    Task<bool> DeleteOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken);
}

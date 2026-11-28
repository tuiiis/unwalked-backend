using Unwalked.Application.Abstractions;
using Unwalked.Domain.Sharing;

namespace Unwalked.UnitTests.Fakes;

public sealed class InMemoryShareRepository : IShareRepository
{
    public Dictionary<Guid, AchievementShare> Shares { get; } = [];

    public HashSet<Guid> DeletedOwners { get; } = [];

    public Task<bool> TryAddAsync(AchievementShare share, CancellationToken cancellationToken)
    {
        if (DeletedOwners.Contains(share.OwnerId))
        {
            return Task.FromResult(false);
        }

        Shares.Add(share.Id, share);
        return Task.FromResult(true);
    }

    public Task<AchievementShare?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Shares.GetValueOrDefault(id));

    public Task<bool> DeleteOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken)
    {
        var owned = Shares.TryGetValue(id, out var share) && share.OwnerId == ownerId;
        return Task.FromResult(owned && Shares.Remove(id));
    }
}

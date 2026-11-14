using Unwalked.Application.Abstractions;
using Unwalked.Domain.Sharing;

namespace Unwalked.UnitTests.Fakes;

public sealed class InMemoryShareRepository : IShareRepository
{
    public Dictionary<Guid, AchievementShare> Shares { get; } = [];

    public Task AddAsync(AchievementShare share, CancellationToken cancellationToken)
    {
        Shares.Add(share.Id, share);
        return Task.CompletedTask;
    }

    public Task<AchievementShare?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Shares.GetValueOrDefault(id));

    public Task<bool> DeleteOwnedAsync(Guid id, Guid ownerId, CancellationToken cancellationToken)
    {
        var owned = Shares.TryGetValue(id, out var share) && share.OwnerId == ownerId;
        return Task.FromResult(owned && Shares.Remove(id));
    }
}

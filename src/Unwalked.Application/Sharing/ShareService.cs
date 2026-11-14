using Unwalked.Application.Abstractions;
using Unwalked.Application.Errors;
using Unwalked.Domain.Common;
using Unwalked.Domain.Sharing;

namespace Unwalked.Application.Sharing;

public sealed class ShareService(IShareRepository shares, IClock clock)
{
    private const string ShareResource = "Share";

    public async Task<ShareDetails> CreateAsync(Guid ownerId, CreateShareCommand command, CancellationToken cancellationToken)
    {
        var name = command.AchievementName.Trim();
        if (name.Length is 0 or > AchievementShare.MaxNameLength)
        {
            throw new DomainValidationException("achievementName", $"Achievement name must be 1-{AchievementShare.MaxNameLength} characters.");
        }

        var now = clock.UtcNow;
        if (command.AchievedOn > DateOnly.FromDateTime(now.UtcDateTime.AddDays(1)))
        {
            throw new DomainValidationException("achievedOn", "Achievement date cannot be in the future.");
        }

        var image = ShareImage.FromBase64(command.ImageContentType, command.ImageBase64);
        var share = new AchievementShare
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            AchievementName = name,
            AchievedOn = command.AchievedOn,
            Image = image.Content,
            ImageContentType = image.ContentType,
            CreatedAt = now,
        };

        await shares.AddAsync(share, cancellationToken);
        return ToDetails(share);
    }

    public async Task<ShareDetails> GetAsync(Guid id, CancellationToken cancellationToken) =>
        ToDetails(await FindExistingAsync(id, cancellationToken));

    public async Task<ShareImage> GetImageAsync(Guid id, CancellationToken cancellationToken)
    {
        var share = await FindExistingAsync(id, cancellationToken);
        return ShareImage.Create(share.ImageContentType, share.Image);
    }

    public async Task DeleteAsync(Guid ownerId, Guid id, CancellationToken cancellationToken)
    {
        if (!await shares.DeleteOwnedAsync(id, ownerId, cancellationToken))
        {
            throw new NotFoundException(ShareResource, id);
        }
    }

    private async Task<AchievementShare> FindExistingAsync(Guid id, CancellationToken cancellationToken) =>
        await shares.FindAsync(id, cancellationToken) ?? throw new NotFoundException(ShareResource, id);

    private static ShareDetails ToDetails(AchievementShare share) =>
        new(share.Id, share.AchievementName, share.AchievedOn, share.ImageContentType, share.CreatedAt);
}

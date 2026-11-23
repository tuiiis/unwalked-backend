namespace Unwalked.Application.Sharing;

/// <summary>
/// Public data of a share without the image bytes and without the owner.
/// </summary>
public sealed record ShareDetails(Guid Id, string AchievementName, DateOnly AchievedOn, string ImageContentType, DateTimeOffset CreatedAt);

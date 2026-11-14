namespace Unwalked.Application.Sharing;

public sealed record ShareDetails(Guid Id, string AchievementName, DateOnly AchievedOn, string ImageContentType, DateTimeOffset CreatedAt);

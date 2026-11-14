namespace Unwalked.Application.Sharing;

public sealed record CreateShareCommand(string AchievementName, DateOnly AchievedOn, string ImageContentType, string ImageBase64);

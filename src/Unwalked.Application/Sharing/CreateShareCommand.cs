namespace Unwalked.Application.Sharing;

/// <summary>
/// Input of the create share use case. The image is still Base64 encoded.
/// </summary>
public sealed record CreateShareCommand(string AchievementName, DateOnly AchievedOn, string ImageContentType, string ImageBase64);

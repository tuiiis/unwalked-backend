namespace Unwalked.Domain.Sharing;

/// <summary>
/// An achievement a user published by link, with an image that has the home area already blurred.
/// </summary>
public sealed class AchievementShare
{
    public const int MaxNameLength = 80;

    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public required string AchievementName { get; set; }

    public DateOnly AchievedOn { get; set; }

    public required byte[] Image { get; set; }

    public required string ImageContentType { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

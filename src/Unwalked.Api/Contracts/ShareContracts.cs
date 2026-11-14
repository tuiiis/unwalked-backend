using System.ComponentModel.DataAnnotations;
using Unwalked.Application.Sharing;

namespace Unwalked.Api.Contracts;

public sealed record ShareImageRequest([Required] string ContentType, [Required] string Data);

public sealed record CreateShareRequest(
    [Required] string AchievementName,
    [Required] DateOnly AchievedOn,
    [Required] ShareImageRequest Image);

public sealed record ShareResponse(Guid Id, string AchievementName, DateOnly AchievedOn, string ImageUrl, DateTimeOffset CreatedAt)
{
    public static ShareResponse From(ShareDetails share, string imageUrl) =>
        new(share.Id, share.AchievementName, share.AchievedOn, imageUrl, share.CreatedAt);
}

using System.ComponentModel.DataAnnotations;
using Unwalked.Application.Sharing;

namespace Unwalked.Api.Contracts;

/// <summary>
/// An achievement image encoded as Base64 together with its declared content type.
/// </summary>
public sealed record ShareImageRequest([Required] string ContentType, [Required] string Data);

/// <summary>
/// Body of a request to publish an achievement.
/// </summary>
public sealed record CreateShareRequest(
    [Required] string AchievementName,
    [Required] DateOnly AchievedOn,
    [Required] ShareImageRequest Image);

/// <summary>
/// Public data of a shared achievement with a link to its image.
/// </summary>
public sealed record ShareResponse(Guid Id, string AchievementName, DateOnly AchievedOn, string ImageUrl, DateTimeOffset CreatedAt)
{
    /// <summary>
    /// Builds the response from share details and the absolute image link.
    /// </summary>
    public static ShareResponse From(ShareDetails share, string imageUrl) =>
        new(share.Id, share.AchievementName, share.AchievedOn, imageUrl, share.CreatedAt);
}

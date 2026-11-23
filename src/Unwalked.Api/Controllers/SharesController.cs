using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Swashbuckle.AspNetCore.Annotations;
using Unwalked.Api.Contracts;
using Unwalked.Application.Sharing;

namespace Unwalked.Api.Controllers;

/// <summary>
/// Public links for shared achievements. Creating and deleting need a bearer token, reading is anonymous.
/// </summary>
[ApiController]
[Route("api/v1/shares")]
public sealed class SharesController(ShareService shares) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [SwaggerOperation(OperationId = "createShare", Summary = "Publishes an achievement with a generalised image and returns its public link.", Description = "Requires a bearer token. The image must be a PNG, JPEG or WebP of at most 300 KB whose bytes match the declared content type. The app is expected to blur the home area before upload.")]
    [ProducesResponseType<ShareResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ShareResponse>> Create(CreateShareRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateShareCommand(request.AchievementName, request.AchievedOn, request.Image.ContentType, request.Image.Data);
        var share = await shares.CreateAsync(CurrentUserId(), command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = share.Id }, ToResponse(share));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [Produces("application/json")]
    [SwaggerOperation(OperationId = "getShare", Summary = "Public achievement data for a share link.", Description = "Anonymous. Returns only the achievement name, date and image link, never the owner.")]
    [ProducesResponseType<ShareResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ShareResponse> Get(Guid id, CancellationToken cancellationToken) =>
        ToResponse(await shares.GetAsync(id, cancellationToken));

    [HttpGet("{id:guid}/image")]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = "getShareImage", Summary = "The generalised achievement image.", Description = "Anonymous. Returns the stored image bytes with their original content type.")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "image/png", "image/jpeg", "image/webp")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<FileContentResult> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var image = await shares.GetImageAsync(id, cancellationToken);
        return File(image.Content, image.ContentType);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [SwaggerOperation(OperationId = "deleteShare", Summary = "Removes a share owned by the caller.", Description = "Requires a bearer token. Shares owned by someone else answer 404 so their existence is not revealed.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await shares.DeleteAsync(CurrentUserId(), id, cancellationToken);
        return NoContent();
    }

    private ShareResponse ToResponse(ShareDetails share) =>
        ShareResponse.From(share, Url.Action(nameof(GetImage), new { id = share.Id })!);

    private Guid CurrentUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

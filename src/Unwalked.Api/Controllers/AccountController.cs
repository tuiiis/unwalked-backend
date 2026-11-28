using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Swashbuckle.AspNetCore.Annotations;
using Unwalked.Application.Auth;

namespace Unwalked.Api.Controllers;

/// <summary>
/// The caller's own account. Deleting it is the server half of erasure, the location history is deleted on the phone.
/// </summary>
[ApiController]
[Route("api/v1/account")]
[Authorize]
public sealed class AccountController(AuthService auth) : ControllerBase
{
    [HttpDelete]
    [SwaggerOperation(OperationId = "deleteAccount", Summary = "Deletes the caller's account and every share it published.", Description = "Requires a bearer token. Shared links stop working at once. Repeating the call after the account is gone still answers 204. Tokens issued before deletion can no longer publish shares.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        await auth.DeleteAccountAsync(CurrentUserId(), cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

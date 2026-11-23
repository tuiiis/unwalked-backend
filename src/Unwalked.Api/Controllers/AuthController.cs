using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Unwalked.Api.Contracts;
using Unwalked.Application.Auth;

namespace Unwalked.Api.Controllers;

/// <summary>
/// Registration and sign-in. Accounts exist only to publish achievements, location history never reaches the server.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    private const string BearerTokenType = "Bearer";

    [HttpPost("register")]
    [SwaggerOperation(OperationId = "register", Summary = "Creates an account used only for sharing achievements.", Description = "The email is trimmed and lower-cased. The password must be 10-128 characters. Registering an existing email answers 409.")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var userId = await auth.RegisterAsync(request.Email, request.Password, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new RegisterResponse(userId));
    }

    [HttpPost("login")]
    [SwaggerOperation(OperationId = "login", Summary = "Exchanges email and password for a JWT access token.", Description = "Wrong email and wrong password answer the same 401 so accounts cannot be enumerated.")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await auth.LoginAsync(request.Email, request.Password, cancellationToken);
        return new LoginResponse(token.Token, BearerTokenType, token.ExpiresAt);
    }
}

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Unwalked.Api.Contracts;
using Unwalked.Application.Areas;

namespace Unwalked.Api.Controllers;

/// <summary>
/// Areas of the catalogue. An area outside the preloaded regions is filled from OpenStreetMap on request.
/// </summary>
[ApiController]
[Route("api/v1/areas")]
[Produces("application/json")]
public sealed class AreasController(AreaService areas) : ControllerBase
{
    [HttpGet("{cell}")]
    [SwaggerOperation(OperationId = "getArea", Summary = "State of an area.", Description = "An area is an H3 cell of resolution 5, about 250 square kilometres. Ready means places for it are in the catalogue.")]
    [ProducesResponseType<AreaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<AreaResponse> Get([SwaggerParameter("H3 cell of resolution 5 as a hex string.", Required = true)] string cell, CancellationToken cancellationToken) =>
        AreaResponse.From(await areas.GetAsync(cell, cancellationToken));

    [HttpPut("{cell}")]
    [SwaggerOperation(OperationId = "ensureArea", Summary = "Make sure an area is in the catalogue.", Description = "Answers 200 when the area is ready. Otherwise starts loading named places of the area from OpenStreetMap and answers 202. Poll the state with GET until it is ready. Idempotent: repeating the request while the area loads starts nothing new.")]
    [ProducesResponseType<AreaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<AreaResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AreaResponse>> Ensure([SwaggerParameter("H3 cell of resolution 5 as a hex string.", Required = true)] string cell, CancellationToken cancellationToken)
    {
        var state = await areas.EnsureAsync(cell, cancellationToken);
        return state.Status == AreaStatus.Ready ? Ok(AreaResponse.From(state)) : Accepted(AreaResponse.From(state));
    }
}

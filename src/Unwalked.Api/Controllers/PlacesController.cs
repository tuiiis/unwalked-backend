using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using Unwalked.Api.Contracts;
using Unwalked.Application.Places;

namespace Unwalked.Api.Controllers;

[ApiController]
[Route("api/v1/places")]
[Produces("application/json")]
public sealed class PlacesController(PlaceQueryService places) : ControllerBase
{
    [HttpGet("near")]
    [SwaggerOperation(OperationId = "findPlacesNear", Summary = "Places within a radius of a point, nearest first.", Description = "Distance is geodesic in metres. A category matches either the primary or an alternate category of a place.")]
    [ProducesResponseType<IReadOnlyList<PlaceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IReadOnlyList<PlaceResponse>> FindNear(
        [FromQuery, BindRequired, SwaggerParameter("Latitude in WGS84 degrees.", Required = true)] double lat,
        [FromQuery, BindRequired, SwaggerParameter("Longitude in WGS84 degrees.", Required = true)] double lon,
        [FromQuery, BindRequired, SwaggerParameter("Search radius in metres, 10-5000.", Required = true)] double radius,
        [FromQuery, SwaggerParameter("Category code matched against primary and alternate categories.")] string? category,
        [FromQuery, SwaggerParameter("Maximum number of places, 1-200, default 50.")] int? limit,
        CancellationToken cancellationToken)
    {
        var found = await places.FindNearAsync(lat, lon, radius, category, limit, cancellationToken);
        return found.Select(PlaceResponse.From).ToList();
    }

    [HttpGet("in-bbox")]
    [SwaggerOperation(OperationId = "findPlacesInBoundingBox", Summary = "Places inside a bounding box, most confident first.", Description = "The box may cover at most 0.25 square degrees, for example 0.5 by 0.5 degrees, about 55 by 32 km around Vilnius.")]
    [ProducesResponseType<IReadOnlyList<PlaceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IReadOnlyList<PlaceResponse>> FindInBoundingBox(
        [FromQuery, BindRequired, SwaggerParameter("South edge latitude.", Required = true)] double minLat,
        [FromQuery, BindRequired, SwaggerParameter("West edge longitude.", Required = true)] double minLon,
        [FromQuery, BindRequired, SwaggerParameter("North edge latitude.", Required = true)] double maxLat,
        [FromQuery, BindRequired, SwaggerParameter("East edge longitude.", Required = true)] double maxLon,
        [FromQuery, SwaggerParameter("Category code.")] string? category,
        [FromQuery, SwaggerParameter("Maximum number of places, 1-200, default 50.")] int? limit,
        CancellationToken cancellationToken)
    {
        var found = await places.FindInBoundingBoxAsync(minLat, minLon, maxLat, maxLon, category, limit, cancellationToken);
        return found.Select(PlaceResponse.From).ToList();
    }

    [HttpPost("by-cells")]
    [Consumes("application/json")]
    [SwaggerOperation(OperationId = "findPlacesByCells", Summary = "Places inside H3 cells of resolution 7-11.", Description = "Each cell is expanded to its resolution 11 children. At most 500 cells and 20,000 expanded cells per request.")]
    [ProducesResponseType<IReadOnlyList<PlaceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IReadOnlyList<PlaceResponse>> FindByCells(PlacesByCellsRequest request, CancellationToken cancellationToken)
    {
        var found = await places.FindInCellsAsync(request.Cells, request.Category, request.Limit, cancellationToken);
        return found.Select(PlaceResponse.From).ToList();
    }
}

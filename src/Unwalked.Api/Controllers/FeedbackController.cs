using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Unwalked.Api.Contracts;
using Unwalked.Application.Feedback;

namespace Unwalked.Api.Controllers;

[ApiController]
[Route("api/v1/feedback")]
[Produces("application/json")]
public sealed class FeedbackController(FeedbackService feedback) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        OperationId = "submitFeedback",
        Summary = "Records anonymous, opt-in feedback on a recommendation.",
        Description = "Idempotent by eventId: repeating an event returns 200 instead of 201 and stores nothing.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(SubmitFeedbackRequest request, CancellationToken cancellationToken)
    {
        var command = new SubmitFeedbackCommand(request.EventId, request.RecommendationType, request.Action, request.Category);
        var stored = await feedback.SubmitAsync(command, cancellationToken);
        return stored ? StatusCode(StatusCodes.Status201Created) : Ok();
    }

    [HttpGet("stats")]
    [SwaggerOperation(OperationId = "getFeedbackStats", Summary = "Acceptance and visit rates per recommendation type.", Description = "Acceptance rate is accepted over accepted plus dismissed. Visit rate is visited over accepted. Rates are null when the denominator is zero. The range may span at most 366 days.")]
    [ProducesResponseType<FeedbackStatsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<FeedbackStatsResponse> Stats(
        [FromQuery, SwaggerParameter("First day, inclusive. Defaults to 30 days before 'to'.")] DateOnly? from,
        [FromQuery, SwaggerParameter("Last day, inclusive. Defaults to today (UTC).")] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var stats = await feedback.GetStatsAsync(from, to, cancellationToken);
        return new FeedbackStatsResponse(from, to, stats.Select(RecommendationTypeStatsResponse.From).ToList());
    }
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Unwalked.Application.Errors;
using Unwalked.Domain.Common;

namespace Unwalked.Api.ErrorHandling;

/// <summary>
/// Turns exceptions from the application and domain layers into Problem Details responses with the matching status code. Unexpected errors are logged and answer 500 without internal details.
/// </summary>
public sealed class ApplicationExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApplicationExceptionHandler> logger)
    : IExceptionHandler
{
    /// <summary>
    /// Writes the Problem Details response for the exception and reports that it was handled.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            DomainValidationException validation => new ValidationProblemDetails(
                new Dictionary<string, string[]> { [validation.Field] = [validation.Message] })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            },
            NotFoundException => Problem(StatusCodes.Status404NotFound, "Resource not found.", exception.Message),
            ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict.", exception.Message),
            AuthenticationFailedException => Problem(StatusCodes.Status401Unauthorized, "Authentication failed.", exception.Message),
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested =>
                Problem(StatusCodes.Status499ClientClosedRequest, "Request cancelled.", null),
            _ => Problem(StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null),
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Request rejected with {StatusCode}: {Reason}", problem.Status, exception.Message);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails Problem(int status, string title, string? detail) =>
        new() { Status = status, Title = title, Detail = detail };
}

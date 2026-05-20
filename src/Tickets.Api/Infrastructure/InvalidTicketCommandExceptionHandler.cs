using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Domain;

namespace Tickets.Api.Infrastructure;

public sealed class InvalidTicketCommandExceptionHandler(
    ILogger<InvalidTicketCommandExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidTicketCommandException invalidCommand)
        {
            return false;
        }

        logger.LogInformation(exception, "Invalid ticket command.");

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid ticket command.",
            Detail = invalidCommand.Message,
            Instance = httpContext.Request.Path
        };

        var wroteProblemDetails = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });

        if (!wroteProblemDetails)
        {
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}

namespace Company.Template.Api.Middleware;

/// <summary>
/// Handles unanticipated exceptions that escape endpoint and application result handling.
/// </summary>
/// <remarks>
/// Expected validation, not-found, and conflict outcomes should remain explicit <c>Result</c> failures. This handler is
/// the last API boundary for exceptional failures, logging the exception while returning a stable problem response.
/// </remarks>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred.");

        var problem = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Detail = "The server encountered an unexpected condition.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}

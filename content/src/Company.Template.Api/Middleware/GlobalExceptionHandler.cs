namespace Company.Template.Api.Middleware;

/// <summary>
///     Handles unanticipated exceptions that escape endpoint and application result handling.
/// </summary>
/// <remarks>
///     Expected validation, not-found, and conflict outcomes should remain explicit <c>Result</c> failures.
///     Request cancellations are not treated as application failures. This handler is the last API boundary
///     for exceptional failures, logging the exception while returning a stable problem response.
/// </remarks>
internal sealed partial class GlobalExceptionHandler : IExceptionHandler
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
        if (exception is OperationCanceledException &&
            httpContext.RequestAborted.IsCancellationRequested)
        {
            LogRequestCancelled(_logger);
            return true;
        }

        if (httpContext.Response.HasStarted)
        {
            LogUnhandledExceptionAfterResponseStarted(_logger, exception);
            return false;
        }

        LogUnhandledException(_logger, exception);

        ProblemDetails problem = new()
        {
            Title = "An unexpected error occurred.",
            Detail = "The server encountered an unexpected condition.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            httpContext.RequestAborted);

        return true;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Debug,
        Message = "Request was cancelled.")]
    private static partial void LogRequestCancelled(ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Unhandled exception occurred after the response had already started.")]
    private static partial void LogUnhandledExceptionAfterResponseStarted(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Unhandled exception occurred.")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception);
}

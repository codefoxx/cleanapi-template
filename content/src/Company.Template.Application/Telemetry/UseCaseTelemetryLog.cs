namespace Company.Template.Application.Telemetry;

internal static partial class UseCaseTelemetryLog
{
    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "Use case {UseCase} completed in {ElapsedMilliseconds} ms.")]
    public static partial void UseCaseCompleted(
        ILogger logger,
        string useCase,
        double elapsedMilliseconds);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Warning,
        Message = "Use case {UseCase} failed with {ErrorType} error {ErrorCode} in {ElapsedMilliseconds} ms.")]
    public static partial void UseCaseFailed(
        ILogger logger,
        string useCase,
        ErrorType errorType,
        string errorCode,
        double elapsedMilliseconds);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "Use case {UseCase} was cancelled after {ElapsedMilliseconds} ms.")]
    public static partial void UseCaseCancelled(
        ILogger logger,
        string useCase,
        double elapsedMilliseconds);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Error,
        Message = "Use case {UseCase} threw an unexpected exception after {ElapsedMilliseconds} ms.")]
    public static partial void UseCaseThrewException(
        ILogger logger,
        Exception exception,
        string useCase,
        double elapsedMilliseconds);
}

using System.Diagnostics;
using Company.Template.Application.Common;
using Company.Template.Application.Diagnostics;

namespace Company.Template.Application.Telemetry;

internal static class UseCaseTelemetry
{
    private const string SuccessOutcome = "success";
    private const string FailureOutcome = "failure";
    private const string CancelledOutcome = "cancelled";
    private const string ExceptionOutcome = "exception";

    public static string GetUseCaseName<TRequest>()
    {
        return typeof(TRequest).Name;
    }

    public static Activity? StartActivity(string useCase)
    {
        Activity? activity = ApplicationTelemetry.ActivitySource.StartActivity(
            useCase,
            ActivityKind.Internal);

        activity?.SetTag("use_case", useCase);

        return activity;
    }

    public static IDisposable? BeginScope(ILogger logger, string useCase)
    {
        return logger.BeginScope(new Dictionary<string, object?>
        {
            ["UseCase"] = useCase
        });
    }

    public static void RecordStarted(string useCase)
    {
        ApplicationTelemetry.UseCasesStarted.Add(
            1,
            new KeyValuePair<string, object?>("use_case", useCase));
    }

    public static T CompleteSuccess<T>(
        ILogger logger,
        Activity? activity,
        string useCase,
        TimeSpan elapsed,
        T result)
    {
        ApplicationTelemetry.UseCasesCompleted.Add(
            1,
            new KeyValuePair<string, object?>("use_case", useCase));

        RecordDuration(useCase, SuccessOutcome, elapsed);

        activity?.SetTag("outcome", SuccessOutcome);
        activity?.SetTag("elapsed_ms", elapsed.TotalMilliseconds);
        activity?.SetStatus(ActivityStatusCode.Ok);

        UseCaseTelemetryLog.UseCaseCompleted(
            logger,
            useCase,
            elapsed.TotalMilliseconds);

        return result;
    }

    public static T CompleteFailure<T>(
        ILogger logger,
        Activity? activity,
        string useCase,
        TimeSpan elapsed,
        Error error,
        T result)
    {
        ApplicationTelemetry.UseCasesFailed.Add(
            1,
            new KeyValuePair<string, object?>("use_case", useCase),
            new KeyValuePair<string, object?>("error_type", error.Type.ToString()));

        RecordDuration(useCase, FailureOutcome, elapsed);

        activity?.SetTag("outcome", FailureOutcome);
        activity?.SetTag("elapsed_ms", elapsed.TotalMilliseconds);
        activity?.SetTag("error.type", error.Type.ToString());
        activity?.SetTag("error.code", error.Code);
        activity?.SetStatus(ActivityStatusCode.Error, error.Code);

        UseCaseTelemetryLog.UseCaseFailed(
            logger,
            useCase,
            error.Type,
            error.Code,
            elapsed.TotalMilliseconds);

        return result;
    }

    public static void CompleteCancellation(
        ILogger logger,
        Activity? activity,
        string useCase,
        TimeSpan elapsed)
    {
        RecordDuration(useCase, CancelledOutcome, elapsed);

        activity?.SetTag("outcome", CancelledOutcome);
        activity?.SetTag("elapsed_ms", elapsed.TotalMilliseconds);
        activity?.SetStatus(ActivityStatusCode.Unset);

        UseCaseTelemetryLog.UseCaseCancelled(
            logger,
            useCase,
            elapsed.TotalMilliseconds);
    }

    public static void CompleteException(
        ILogger logger,
        Activity? activity,
        string useCase,
        TimeSpan elapsed,
        Exception exception)
    {
        ApplicationTelemetry.UseCasesFailed.Add(
            1,
            new KeyValuePair<string, object?>("use_case", useCase),
            new KeyValuePair<string, object?>("error_type", "exception"));

        RecordDuration(useCase, ExceptionOutcome, elapsed);

        activity?.SetTag("outcome", ExceptionOutcome);
        activity?.SetTag("elapsed_ms", elapsed.TotalMilliseconds);
        activity?.SetTag("exception.type", exception.GetType().Name);
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

        UseCaseTelemetryLog.UseCaseThrewException(
            logger,
            exception,
            useCase,
            elapsed.TotalMilliseconds);
    }

    private static void RecordDuration(
        string useCase,
        string outcome,
        TimeSpan elapsed)
    {
        ApplicationTelemetry.UseCaseDuration.Record(
            elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("use_case", useCase),
            new KeyValuePair<string, object?>("outcome", outcome));
    }
}

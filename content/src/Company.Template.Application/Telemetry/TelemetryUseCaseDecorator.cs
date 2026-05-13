using System.Diagnostics;
using Company.Template.Application.Abstractions;

namespace Company.Template.Application.Telemetry;

/// <summary>
///     Adds telemetry around value-returning application use cases without changing their workflow contract.
/// </summary>
/// <remarks>
///     The decorator records activity, metrics, logs, and failure outcomes at the application boundary while
///     preserving the explicit <see cref="Result{T}" /> semantics used by the inner use case.
/// </remarks>
public sealed class TelemetryUseCaseDecorator<TRequest, TResult> : IUseCase<TRequest, TResult>
{
    private readonly IUseCase<TRequest, TResult> _inner;
    private readonly ILogger<TelemetryUseCaseDecorator<TRequest, TResult>> _logger;

    public TelemetryUseCaseDecorator(
        IUseCase<TRequest, TResult> inner,
        ILogger<TelemetryUseCaseDecorator<TRequest, TResult>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResult>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        string useCase = UseCaseTelemetry.GetUseCaseName<TRequest>();

        using Activity? activity = UseCaseTelemetry.StartActivity(useCase);
        using IDisposable? scope = UseCaseTelemetry.BeginScope(_logger, useCase);

        UseCaseTelemetry.RecordStarted(useCase);

        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            Result<TResult> result = await _inner.ExecuteAsync(request, cancellationToken);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            return result switch
            {
                { IsSuccess: true } => UseCaseTelemetry.CompleteSuccess(
                    _logger,
                    activity,
                    useCase,
                    elapsed,
                    result),

                { Error: { } error } => UseCaseTelemetry.CompleteFailure(
                    _logger,
                    activity,
                    useCase,
                    elapsed,
                    error,
                    result),

                _ => throw new InvalidOperationException(
                    $"Failed {nameof(Result<>)} must contain an error.")
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            UseCaseTelemetry.CompleteCancellation(
                _logger,
                activity,
                useCase,
                elapsed);

            throw;
        }
        catch (Exception exception)
        {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            UseCaseTelemetry.CompleteException(
                _logger,
                activity,
                useCase,
                elapsed,
                exception);

            throw;
        }
    }
}

/// <summary>
///     Adds telemetry around command-style application use cases without changing their workflow contract.
/// </summary>
/// <remarks>
///     The decorator observes successful, failed, cancelled, and exceptional executions consistently while leaving
///     expected application failures represented as <see cref="Result" /> values.
/// </remarks>
public sealed class TelemetryUseCaseDecorator<TRequest> : IUseCase<TRequest>
{
    private readonly IUseCase<TRequest> _inner;
    private readonly ILogger<TelemetryUseCaseDecorator<TRequest>> _logger;

    public TelemetryUseCaseDecorator(
        IUseCase<TRequest> inner,
        ILogger<TelemetryUseCaseDecorator<TRequest>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        string useCase = UseCaseTelemetry.GetUseCaseName<TRequest>();

        using Activity? activity = UseCaseTelemetry.StartActivity(useCase);
        using IDisposable? scope = UseCaseTelemetry.BeginScope(_logger, useCase);

        UseCaseTelemetry.RecordStarted(useCase);

        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            Result result = await _inner.ExecuteAsync(request, cancellationToken);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            return result switch
            {
                { IsSuccess: true } => UseCaseTelemetry.CompleteSuccess(
                    _logger,
                    activity,
                    useCase,
                    elapsed,
                    result),

                { Error: { } error } => UseCaseTelemetry.CompleteFailure(
                    _logger,
                    activity,
                    useCase,
                    elapsed,
                    error,
                    result),

                _ => throw new InvalidOperationException(
                    $"Failed {nameof(Result)} must contain an error.")
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            UseCaseTelemetry.CompleteCancellation(
                _logger,
                activity,
                useCase,
                elapsed);

            throw;
        }
        catch (Exception exception)
        {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            UseCaseTelemetry.CompleteException(
                _logger,
                activity,
                useCase,
                elapsed,
                exception);

            throw;
        }
    }
}

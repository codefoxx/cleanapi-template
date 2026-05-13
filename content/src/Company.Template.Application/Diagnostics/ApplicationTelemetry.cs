using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Company.Template.Application.Diagnostics;

public static class ApplicationTelemetry
{
    public const string ActivitySourceName = "Company.Template.Application";
    public const string MeterName = "Company.Template.Application";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);

    public static readonly Histogram<double> UseCaseDuration =
        Meter.CreateHistogram<double>(
            "application.usecases.duration",
            "ms");

    public static readonly Counter<long> UseCasesCompleted =
        Meter.CreateCounter<long>("application.usecases.completed");

    public static readonly Counter<long> UseCasesFailed =
        Meter.CreateCounter<long>("application.usecases.failed");

    public static readonly Counter<long> UseCasesStarted =
        Meter.CreateCounter<long>("application.usecases.started");
}

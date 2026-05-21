using Company.Template.Application.Abstractions.Time;
using Company.Template.Infrastructure.Time;

namespace Company.Template.Infrastructure;

/// <summary>
///     Registers infrastructure-backed services for shared cross-cutting concerns.
/// </summary>
/// <remarks>
///     The system clock is a technical cross-cutting service that keeps time access testable and independent from direct
///     system calls in application workflows.
/// </remarks>
public sealed class InfrastructureCrossCuttingModule : IFeatureServiceModule<InfrastructureCrossCuttingFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddSingleton<IClock, SystemClock>();
    }
}

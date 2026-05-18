using Company.Template.Application.Telemetry;
using Company.Template.Composition.Features;
using Company.Template.Composition.Framework;

namespace Company.Template.Application;

/// <summary>
///     Registers application-wide cross-cutting behavior.
/// </summary>
/// <remarks>
///     Feature-specific use cases are registered before this module is activated so decorators can be applied to the
///     selected application workflows consistently.
/// </remarks>
public sealed class ApplicationCrossCuttingModule : IFeatureServiceModule<CrossCuttingConcerns>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.Decorate(typeof(IUseCase<,>), typeof(TelemetryUseCaseDecorator<,>));
        context.Services.Decorate(typeof(IUseCase<>), typeof(TelemetryUseCaseDecorator<>));
    }
}

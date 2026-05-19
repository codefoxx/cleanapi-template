namespace Company.Template.Application.Telemetry;

/// <summary>
///     Applies telemetry decorators to application use-case services after selected use cases have been registered.
/// </summary>
public sealed class UseCaseTelemetryDecoratorModule
    : IFeatureServiceDecoratorModule<ApplicationUseCasesFeature, UseCaseTelemetryFeature>
{
    public void Decorate(FeatureServiceContext context)
    {
        context.Services.Decorate(typeof(IUseCase<,>), typeof(TelemetryUseCaseDecorator<,>));
        context.Services.Decorate(typeof(IUseCase<>), typeof(TelemetryUseCaseDecorator<>));
    }
}

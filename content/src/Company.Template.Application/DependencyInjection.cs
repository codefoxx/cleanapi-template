using Company.Template.Application.Telemetry;

namespace Company.Template.Application;

/// <summary>
///     Registers application-layer cross-cutting services.
/// </summary>
/// <remarks>
///     Feature-specific use cases are registered by feature modules. Application-level decorators stay here so they can
///     be applied consistently after selected features have registered their use cases.
/// </remarks>
public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.Decorate(typeof(IUseCase<,>), typeof(TelemetryUseCaseDecorator<,>));
            services.Decorate(typeof(IUseCase<>), typeof(TelemetryUseCaseDecorator<>));

            return services;
        }
    }
}

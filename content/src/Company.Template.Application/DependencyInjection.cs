using System.Reflection;
using Company.Template.Application.Abstractions;
using Company.Template.Application.Telemetry;

namespace Company.Template.Application;

/// <summary>
/// Registers application-layer use cases and their cross-cutting decorators.
/// </summary>
/// <remarks>
/// Use case discovery is assembly-based so new application workflows can be composed without endpoint code depending on
/// concrete implementations. Telemetry is applied as a decorator, preserving the use case contract.
/// </remarks>
public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddUseCasesFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }

        private IServiceCollection AddUseCasesFromAssembly(Assembly assembly)
        {
            services.Scan(scan => scan
                .FromAssemblyDependencies(assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IUseCase<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IUseCase<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Decorate(typeof(IUseCase<,>), typeof(TelemetryUseCaseDecorator<,>));
            services.Decorate(typeof(IUseCase<>), typeof(TelemetryUseCaseDecorator<>));

            return services;
        }
    }
}

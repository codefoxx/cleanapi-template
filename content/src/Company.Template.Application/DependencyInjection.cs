using System.Reflection;
using Company.Template.Application.Abstractions;
using Company.Template.Application.Telemetry;

namespace Company.Template.Application;

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

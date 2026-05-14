using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Application.Abstractions.Persistence;
using Company.Template.Application.Abstractions.Time;
using Company.Template.Application.Products;
using Company.Template.Infrastructure.DomainEvents;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Time;

namespace Company.Template.Infrastructure;

/// <summary>
///     Wires infrastructure implementations to application-layer abstractions.
/// </summary>
/// <remarks>
///     The registration keeps composition concerns in infrastructure: database setup,
///     persistence implementation, domain-event dispatching, and the clock implementation
///     are provided here while application use cases depend on abstractions.
/// </remarks>
public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddTemplateDatabase(configuration);

            services.AddScoped<IUnitOfWork>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
            services.AddSingleton<IClock, SystemClock>();

            services.Scan(scan => scan
                                 .FromAssemblyOf<InfrastructureAssemblyMarker>()
                                 .AddClasses(classes => classes.AssignableTo<IQuery>())
                                 .AsImplementedInterfaces()
                                 .WithScopedLifetime());

            return services;
        }
    }
}

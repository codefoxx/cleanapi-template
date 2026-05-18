using Company.Template.Application.Abstractions.Persistence;
using Company.Template.Infrastructure.Persistence;

namespace Company.Template.Infrastructure;

/// <summary>
///     Wires shared infrastructure implementations to application-layer abstractions.
/// </summary>
/// <remarks>
///     Feature-specific adapters are registered by feature modules. Shared infrastructure services stay here because they
///     support the application as a whole rather than a single feature.
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

            return services;
        }
    }
}

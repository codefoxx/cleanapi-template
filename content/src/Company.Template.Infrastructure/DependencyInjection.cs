using Company.Template.Application.Abstractions;
using Company.Template.Infrastructure.DomainEvents;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();

        return services;
    }
}

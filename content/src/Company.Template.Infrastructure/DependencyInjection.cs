using Company.Template.Application.Abstractions;
using Company.Template.Infrastructure.DomainEvents;
using Company.Template.Infrastructure.Options;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Persistence.Providers;
using Company.Template.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Company.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .Validate(options => DatabaseProvider.IsSupported(options.Provider),
                "Database:Provider is not supported.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                "Database:ConnectionStringName is required.")
            .ValidateOnStart();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            DatabaseOptions databaseOptions = serviceProvider
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            var connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{databaseOptions.ConnectionStringName}' is missing.");
            }

            DatabaseProviderConfigurator.Configure(optionsBuilder, connectionString);
        });

        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}

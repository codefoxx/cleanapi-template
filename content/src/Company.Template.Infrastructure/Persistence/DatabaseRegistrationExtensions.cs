using Company.Template.Infrastructure.Options;
using Company.Template.Infrastructure.Persistence.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Company.Template.Infrastructure.Persistence;

public static class DatabaseRegistrationExtensions
{
    public static IServiceCollection AddTemplateDatabase(
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

        return services;
    }
}

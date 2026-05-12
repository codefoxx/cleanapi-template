using Company.Template.Infrastructure.Options;
using Company.Template.Infrastructure.Persistence.Providers;

namespace Company.Template.Infrastructure.Persistence;

/// <summary>
/// Registers the EF Core database implementation selected by infrastructure configuration.
/// </summary>
/// <remarks>
/// Provider choice and connection-string lookup are resolved during composition. Missing or unsupported configuration is
/// treated as a startup failure rather than leaking provider decisions into application or domain code.
/// </remarks>
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

            string? connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName);

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

        return services;
    }
}

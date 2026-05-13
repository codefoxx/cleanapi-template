using Company.Template.Infrastructure.Options;
using Company.Template.Infrastructure.Persistence.Providers;

namespace Company.Template.Infrastructure.Persistence;

/// <summary>
///     Registers the EF Core database implementation selected when the template was generated.
/// </summary>
/// <remarks>
///     The database provider is a template/build-time choice. Runtime configuration selects the
///     connection string, but it must match the generated provider to fail fast on configuration drift.
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
           .Validate(options => DatabaseProvider.MatchesSelectedProvider(options.Provider),
                $"Database:Provider must match the generated provider '{DatabaseProvider.SelectedProvider}'.")
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

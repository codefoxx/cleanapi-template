using Company.Template.Application.Abstractions.Persistence;
using Company.Template.Infrastructure.Options;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Persistence.Providers;

namespace Company.Template.Infrastructure;

/// <summary>
///     Registers the persistence adapter used by application workflows.
/// </summary>
/// <remarks>
///     The module owns EF Core database registration and the unit-of-work adapter so persistence is activated explicitly
///     from executable composition roots.
/// </remarks>
public sealed class InfrastructurePersistenceModule : IFeatureServiceModule<PersistenceFeature>
{
    public void Register(FeatureServiceContext context)
    {
        IConfiguration configuration = context.RequireConfiguration();

        context.Services
               .AddOptions<DatabaseOptions>()
               .BindConfiguration(DatabaseOptions.SectionName)
               .Validate(options => DatabaseProvider.MatchesSelectedProvider(options.Provider),
                    $"Database:Provider must match the generated provider '{DatabaseProvider.SelectedProvider}'.")
               .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                    "Database:ConnectionStringName is required.")
               .ValidateOnStart();

        context.Services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
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

        context.Services
               .AddHealthChecks()
               .AddDbContextCheck<ApplicationDbContext>();

        context.Services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
    }
}

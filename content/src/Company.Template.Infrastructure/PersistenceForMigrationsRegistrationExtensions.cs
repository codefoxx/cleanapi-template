using Company.Template.Composition.Framework;

namespace Company.Template.Infrastructure;

/// <summary>
///     Registers the persistence services needed by migration helper processes.
/// </summary>
/// <remarks>
///     Migrations construct the application DbContext directly, so the migration process also needs the minimal
///     domain-event infrastructure required by the DbContext constructor.
/// </remarks>
public static class PersistenceForMigrationsRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPersistenceForMigrations(IConfiguration configuration)
        {
            FeatureServiceContext context = new(
                services,
                [typeof(InfrastructureAssemblyMarker).Assembly],
                configuration);

            new InfrastructureDomainEventsModule().Register(context);
            new InfrastructurePersistenceModule().Register(context);

            return services;
        }
    }
}

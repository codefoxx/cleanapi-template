using Company.Template.Composition.Framework;

namespace Company.Template.Infrastructure;

/// <summary>
///     Registers the persistence feature for executable projects that do not use the main composition root.
/// </summary>
/// <remarks>
///     This keeps helper processes from constructing feature service contexts directly while preserving explicit
///     persistence feature activation.
/// </remarks>
public static class PersistenceFeatureRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMigrationPersistence(IConfiguration configuration)
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

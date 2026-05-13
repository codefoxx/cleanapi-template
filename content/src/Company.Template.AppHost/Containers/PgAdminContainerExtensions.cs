namespace Company.Template.AppHost.Containers;

/// <summary>
///     Adds the optional pgAdmin container used by local Aspire orchestration.
/// </summary>
/// <remarks>
///     The helper isolates development-time database administration from the application runtime so enabling the container
///     does not alter application persistence behavior or domain code.
/// </remarks>
public static class PgAdminContainerExtensions
{
    public static IResourceBuilder<ContainerResource> AddPgAdminContainer(
        this IDistributedApplicationBuilder builder,
        Action<PgAdminContainerOptions>? configure = null)
    {
        PgAdminContainerOptions options = new();
        configure?.Invoke(options);

        Validate(options);

        return builder
              .AddContainer(options.ResourceName, options.Image)
              .WithEnvironment("PGADMIN_DEFAULT_EMAIL", options.DefaultEmail)
              .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", options.DefaultPassword)
              .WithHttpEndpoint(
                   targetPort: options.TargetPort,
                   name: options.EndpointName);
    }

    private static void Validate(PgAdminContainerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ResourceName))
        {
            throw new InvalidOperationException("pgAdmin container name is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Image))
        {
            throw new InvalidOperationException("pgAdmin image is required.");
        }

        if (string.IsNullOrWhiteSpace(options.DefaultEmail))
        {
            throw new InvalidOperationException("pgAdmin default email is required.");
        }

        if (string.IsNullOrWhiteSpace(options.DefaultPassword))
        {
            throw new InvalidOperationException("pgAdmin default password is required.");
        }
    }
}

namespace Company.Template.AppHost.Containers;

/// <summary>
///     Adds the local Keycloak resource used by the Aspire AppHost.
/// </summary>
/// <remarks>
///     These helpers keep identity-provider wiring at the local orchestration boundary: the API receives authentication
///     settings through environment variables while application workflows remain independent of the container resource.
/// </remarks>
public static class KeycloakContainerExtensions
{
    public static KeycloakResourceRegistration AddTemplateKeycloak(
        this IDistributedApplicationBuilder builder,
        Action<KeycloakContainerOptions>? configure = null)
    {
        KeycloakContainerOptions options = new();
        configure?.Invoke(options);

        Validate(options);

        IResourceBuilder<KeycloakResource> keycloak = builder
                                                     .AddKeycloak(options.ResourceName, options.Port)
                                                     .WithRealmImport(options.RealmImportPath);

        if (options.UseDataVolume)
        {
            keycloak = keycloak.WithDataVolume();
        }

        return new KeycloakResourceRegistration(keycloak, options);
    }

    public static IResourceBuilder<ProjectResource> WithTemplateKeycloakAuthentication(
        this IResourceBuilder<ProjectResource> api,
        KeycloakResourceRegistration keycloak)
    {
        return api
              .WithReference(keycloak.Resource)
              .WaitFor(keycloak.Resource)
              .WithEnvironment("Authentication__Authority", keycloak.Options.Authority)
              .WithEnvironment("Authentication__Audience", keycloak.Options.Audience);
    }

    private static void Validate(KeycloakContainerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ResourceName))
        {
            throw new InvalidOperationException("Keycloak resource name is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Realm))
        {
            throw new InvalidOperationException("Keycloak realm is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Keycloak audience is required.");
        }
    }
}
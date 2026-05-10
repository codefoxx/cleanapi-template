namespace Company.Template.AppHost.Containers;

public static class KeycloakContainerExtensions
{
    public static KeycloakResourceRegistration AddTemplateKeycloak(
        this IDistributedApplicationBuilder builder,
        Action<KeycloakContainerOptions>? configure = null)
    {
        var options = new KeycloakContainerOptions();
        configure?.Invoke(options);

        Validate(options);

        IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak(options.ResourceName, options.Port);

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
            .WithEnvironment("Authentication__Enabled", "true")
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

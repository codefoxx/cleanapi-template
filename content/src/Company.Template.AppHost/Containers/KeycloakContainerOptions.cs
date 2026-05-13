namespace Company.Template.AppHost.Containers;

public sealed class KeycloakContainerOptions
{
    public string Audience { get; set; } = AppHostNames.AuthAudience;
    public string Authority => $"http://localhost:{Port}/realms/{Realm}";
    public int Port { get; set; } = 8080;
    public string Realm { get; set; } = AppHostNames.KeycloakRealm;
    public string RealmImportPath { get; set; } = "../../infra/keycloak/realms";
    public string ResourceName { get; set; } = AppHostNames.KeycloakResourceName;
    public bool UseDataVolume { get; set; } = true;
}

public sealed record KeycloakResourceRegistration(IResourceBuilder<KeycloakResource> Resource, KeycloakContainerOptions Options);

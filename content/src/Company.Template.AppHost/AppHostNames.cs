namespace Company.Template.AppHost;

internal static class AppHostNames
{
    public const string DatabaseProvider = "__DB_PROVIDER__";

    private const string RawApiResourceName = "__API_RESOURCE_NAME__";
    private const string RawAuthAudience = "__AUTH_AUDIENCE__";
    private const string RawDatabaseResourceName = "__DATABASE_RESOURCE_NAME__";
    private const string RawKeycloakRealm = "__KEYCLOAK_REALM__";
    private const string RawKeycloakResourceName = "__KEYCLOAK_RESOURCE_NAME__";
    private const string RawMigrationServiceResourceName = "__MIGRATION_SERVICE_RESOURCE_NAME__";

    public static string ApiResourceName { get; } = Normalize(RawApiResourceName);
    public static string AuthAudience { get; } = Normalize(RawAuthAudience);
    public static string DatabaseResourceName { get; } = Normalize(RawDatabaseResourceName);
    public static string KeycloakRealm { get; } = Normalize(RawKeycloakRealm);
    public static string KeycloakResourceName { get; } = Normalize(RawKeycloakResourceName);
    public static string MigrationServiceResourceName { get; } = Normalize(RawMigrationServiceResourceName);

    private static string Normalize(string value)
    {
        string normalized = value
                           .Trim()
                           .Replace(".", "-", StringComparison.Ordinal)
                           .Replace("_", "-", StringComparison.Ordinal)
                           .Replace(" ", "-", StringComparison.Ordinal)
                           .ToLowerInvariant();

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        return normalized.Trim('-');
    }
}
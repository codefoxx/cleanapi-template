namespace Company.Template.AppHost;

internal static class AppHostNames
{
    public const string DatabaseProvider = "__DB_PROVIDER__";

    private const string RawApiResourceName = "__API_RESOURCE_NAME__";
    private const string RawDatabaseResourceName = "__DATABASE_RESOURCE_NAME__";
    private const string RawMigrationServiceResourceName = "__MIGRATION_SERVICE_RESOURCE_NAME__";

    public static string ApiResourceName { get; } = Normalize(RawApiResourceName);
    public static string DatabaseResourceName { get; } = Normalize(RawDatabaseResourceName);
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
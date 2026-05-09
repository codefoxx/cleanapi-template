namespace Company.Template.Infrastructure.Persistence.Providers;

internal static class DatabaseProviderConfigurator
{
    public static void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}

namespace Company.Template.Infrastructure.Persistence.Providers;

/// <summary>
///     Applies the EF Core provider configuration selected when the template was generated.
/// </summary>
/// <remarks>
///     Only one provider configurator is compiled into the generated project. The selected provider
///     is a generation-time choice, while runtime configuration only supplies the connection string.
/// </remarks>
internal static class DatabaseProviderConfigurator
{
    public static void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}

using Company.Template.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure.Persistence;

public static class DatabaseRegistrationExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        if (!DatabaseProvider.IsSupported(databaseOptions.Provider))
        {
            throw new InvalidOperationException(
                $"Unsupported database provider '{databaseOptions.Provider}'. This template was generated for '{DatabaseProvider.SelectedProvider}'.");
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
        }

        return SelectedDatabaseProvider.AddDatabase(services, connectionString);
    }
}

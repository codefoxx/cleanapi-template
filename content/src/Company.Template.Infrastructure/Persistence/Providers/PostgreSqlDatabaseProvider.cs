using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure.Persistence;

internal static class SelectedDatabaseProvider
{
    public static IServiceCollection AddDatabase(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");

        return services;
    }
}

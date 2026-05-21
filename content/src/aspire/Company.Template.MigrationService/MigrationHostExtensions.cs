using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.MigrationService;

internal static class MigrationHostExtensions
{
    public static async Task<int> RunDatabaseMigrationAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = host.Services.CreateScope();

        ApplicationDbContext dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        ILogger<Program> logger = scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

#pragma warning disable CA1031 // Process boundary: log migration failure and return non-zero exit code.
        try
        {
            DatabaseMigrationLog.StartingMigration(logger);

            await dbContext.Database.MigrateAsync(cancellationToken);

            DatabaseMigrationLog.MigrationCompleted(logger);

            return 0;
        }
        catch (Exception exception)
        {
            DatabaseMigrationLog.MigrationFailed(logger, exception);

            return 1;
        }
#pragma warning restore CA1031
    }
}

internal static partial class DatabaseMigrationLog
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Information,
        Message = "Starting database migration.")]
    public static partial void StartingMigration(
        ILogger logger);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Database migration completed successfully.")]
    public static partial void MigrationCompleted(
        ILogger logger);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Error,
        Message = "Database migration failed.")]
    public static partial void MigrationFailed(
        ILogger logger,
        Exception exception);
}

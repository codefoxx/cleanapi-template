using Company.Template.Composition.Abstractions.Features;
using Company.Template.Infrastructure;
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

FeatureServiceContext persistenceContext = new(
    builder.Services,
    [typeof(InfrastructureAssemblyMarker).Assembly],
    builder.Configuration);

new InfrastructurePersistenceModule().Register(persistenceContext);

using IHost host = builder.Build();

using IServiceScope scope = host.Services.CreateScope();

ApplicationDbContext dbContext = scope.ServiceProvider
                                      .GetRequiredService<ApplicationDbContext>();

ILogger<Program> logger = scope.ServiceProvider
                               .GetRequiredService<ILogger<Program>>();

#pragma warning disable CA1031 // Process boundary: log migration failure and return non-zero exit code.
try
{
    DatabaseMigrationLog.StartingMigration(logger);

    await dbContext.Database.MigrateAsync();

    DatabaseMigrationLog.MigrationCompleted(logger);
}
catch (Exception exception)
{
    DatabaseMigrationLog.MigrationFailed(logger, exception);
    Environment.ExitCode = 1;
}
#pragma warning restore CA1031

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

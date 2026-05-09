using Company.Template.Infrastructure;
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);

using IHost host = builder.Build();

using IServiceScope scope = host.Services.CreateScope();

ApplicationDbContext dbContext = scope.ServiceProvider
    .GetRequiredService<ApplicationDbContext>();

ILogger<Program> logger = scope.ServiceProvider
    .GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting database migration.");

    await dbContext.Database.MigrateAsync();

    logger.LogInformation("Database migration completed successfully.");
}
catch (Exception exception)
{
    logger.LogError(exception, "Database migration failed.");
    Environment.ExitCode = 1;
}

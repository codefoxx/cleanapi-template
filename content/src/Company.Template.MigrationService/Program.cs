using Company.Template.Infrastructure;
using Company.Template.MigrationService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddPersistenceFeature(builder.Configuration);

using IHost host = builder.Build();

return await host.RunDatabaseMigrationAsync();

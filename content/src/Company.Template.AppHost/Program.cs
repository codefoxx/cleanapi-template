using Company.Template.AppHost;
using Company.Template.AppHost.Containers;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

bool startPgAdmin = builder.Configuration.GetValue<bool>("AppHost:StartPgAdmin");
bool startKeycloak = builder.Configuration.GetValue<bool>("AppHost:StartKeycloak");

IResourceBuilder<IResourceWithConnectionString> database = AspireDatabase.Create(builder);

IResourceBuilder<ProjectResource> migrationService = builder
                                                    .AddProject<Company_Template_MigrationService>(AppHostNames.MigrationServiceResourceName)
                                                    .WithReference(database)
                                                    .WaitFor(database)
                                                    .WithEnvironment("Database__Provider", AppHostNames.DatabaseProvider);

IResourceBuilder<ProjectResource> api = builder
                                       .AddProject<Company_Template_Api>(AppHostNames.ApiResourceName)
                                       .WithReference(database)
                                       .WaitFor(database)
                                       .WaitForCompletion(migrationService)
                                       .WithEnvironment("Database__Provider", AppHostNames.DatabaseProvider);

if (startPgAdmin)
{
    builder.AddPgAdminContainer();
}

if (startKeycloak)
{
    KeycloakResourceRegistration keycloak = builder.AddTemplateKeycloak();

    api.WithTemplateKeycloakAuthentication(keycloak);
}

builder.Build().Run();

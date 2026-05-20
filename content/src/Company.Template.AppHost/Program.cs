using Company.Template.AppHost;
using Company.Template.AppHost.Containers;
using Company.Template.AppHost.Providers;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<IResourceWithConnectionString> database = AspireDatabase.Create(builder);

IResourceBuilder<ProjectResource> migrationService = builder
                                                    .AddProject<Company_Template_MigrationService>(AppHostNames.MigrationServiceResourceName)
                                                    .WithReference(database)
                                                    .WaitFor(database)
                                                    .WithEnvironment("Database__Provider", AppHostNames.DatabaseProvider);

IResourceBuilder<ProjectResource> api = builder
                                       .AddProject<Company_Template_Composition>(AppHostNames.ApiResourceName)
                                       .WithReference(database)
                                       .WaitFor(database)
                                       .WaitForCompletion(migrationService)
                                       .WithEnvironment("Database__Provider", AppHostNames.DatabaseProvider);

KeycloakResourceRegistration keycloak = builder.AddTemplateKeycloak();

api.WithTemplateKeycloakAuthentication(keycloak);

builder.Build().Run();
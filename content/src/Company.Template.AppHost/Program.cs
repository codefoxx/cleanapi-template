using Company.Template.AppHost;
//#if (auth == "Keycloak")
using Company.Template.AppHost.Containers;
//#endif
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
//#if (auth == "Keycloak")
KeycloakResourceRegistration keycloak = builder.AddTemplateKeycloak();

api.WithTemplateKeycloakAuthentication(keycloak);
//#endif

builder.Build().Run();
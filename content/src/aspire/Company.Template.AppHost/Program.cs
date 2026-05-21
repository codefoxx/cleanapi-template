using Company.Template.AppHost;
using Company.Template.AppHost.Containers;
using Company.Template.AppHost.Providers;
using Projects;

//#if (auth == "Keycloak")
//#endif

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<IResourceWithConnectionString> database = AspireDatabase.Create(builder);

IResourceBuilder<ProjectResource> migrationService = builder
    .AddProject<Company_Template_MigrationService>(AppHostNames.MigrationServiceResourceName)
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Database__Provider", AppHostNames.DatabaseProvider);

//#if (auth == "Keycloak")
KeycloakResourceRegistration keycloak = builder.AddTemplateKeycloak();

//#endif
builder
    .AddProject<Company_Template_CompositionRoot>(AppHostNames.ApiResourceName)
    .WithReference(database)
    //#if (auth == "Keycloak")
    .WithTemplateKeycloakAuthentication(keycloak)
    //#endif
    .WaitFor(database)
    .WaitForCompletion(migrationService)
    .WithEnvironment("Database__Provider", AppHostNames.DatabaseProvider);

builder.Build().Run();

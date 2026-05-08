using Aspire.Hosting.ApplicationModel;
using Company.Template.AppHost.Providers;

var builder = DistributedApplication.CreateBuilder(args);

const string databaseProvider = "__DB_PROVIDER__";
const bool enableKeycloak = false;

IResourceBuilder<IResourceWithConnectionString> database = AspireDatabase.Create(builder);

var api = builder
    .AddProject<Projects.Company_Template_Api>("company-template-api")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Database__Provider", databaseProvider);

if (enableKeycloak)
{
    var keycloak = builder
        .AddKeycloak("keycloak", 8080)
        .WithDataVolume();

    api
        .WithReference(keycloak)
        .WaitFor(keycloak)
        .WithEnvironment("Authentication__Enabled", "true")
        .WithEnvironment("Authentication__Authority", "http://localhost:8080/realms/company-template")
        .WithEnvironment("Authentication__Audience", "company-template-api");
}

builder.Build().Run();

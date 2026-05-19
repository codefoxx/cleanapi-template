using Company.Template.Api;
using Company.Template.Application;
using Company.Template.Infrastructure;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext();
});

builder.AddServiceDefaults();

builder.Services
       .AddFeatureServicesFromAssemblies(
            typeof(ApiAssemblyMarker).Assembly,
            typeof(ApplicationAssemblyMarker).Assembly,
            typeof(InfrastructureAssemblyMarker).Assembly)
       .WithConfiguration(builder.Configuration)
       .ComposeFeatures(features => features
           .AddTemplateDefaults()
           .AddProductCatalog()
           .DecorateUseCasesWithTelemetry());

builder.Services.AddApiAdapter();

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();

FeatureWebAppBuilder webAppFeatures = app
                                     .UseFeaturesFromAssemblies(typeof(ApiAssemblyMarker).Assembly)
                                     .Use<CrossCuttingConcerns>()
                                     .Use<OpenApiFeature>();

app.UseApiAdapter();

webAppFeatures.Use<ProductsFeature>();

app.Run();

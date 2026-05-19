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

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();

app.UseFeaturesFromAssemblies(typeof(ApiAssemblyMarker).Assembly)
   .Use<CrossCuttingConcerns>()
   .Use<OpenApiFeature>()
   .Use<ApiAdapterFeature>()
   .Use<ProductsFeature>();

app.Run();

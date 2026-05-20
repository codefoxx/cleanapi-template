using Company.Template.Api;
using Company.Template.Application;
using Company.Template.Infrastructure;
using Serilog;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Assembly[] serviceFeatureAssemblies =
[
    typeof(ApiAssemblyMarker).Assembly,
    typeof(ApplicationAssemblyMarker).Assembly,
    typeof(InfrastructureAssemblyMarker).Assembly
];

Assembly[] webAppFeatureAssemblies =
[
    typeof(ApiAssemblyMarker).Assembly
];

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext();
});

builder.AddServiceDefaults();

builder.Services
       .AddFeatureServicesFromAssemblies(serviceFeatureAssemblies)
       .WithConfiguration(builder.Configuration)
       .ComposeFeatures(features => features
           .AddTemplateDefaults()
           .AddProductCatalog()
           .DecorateUseCasesWithTelemetry());

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();

app.UseFeaturesFromAssemblies(webAppFeatureAssemblies)
   .Use<CrossCuttingConcerns>()
   .Use<OpenApiFeature>()
   .Use<ApiAdapterFeature>()
   .Use<ProductsFeature>();

app.Run();
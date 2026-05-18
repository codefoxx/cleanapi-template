using Company.Template.Api;
using Company.Template.Application;
using Company.Template.Application.Products;
using Company.Template.Composition.Abstractions.Features;
using Company.Template.Composition.Features;
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
       .Add<PersistenceFeature>()
       .Add<ProductsFeature>()
       .Add<CrossCuttingConcerns>()
       .Add<DomainEventsFeature>();

builder.Services.AddApiAdapter();

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();

FeatureWebAppBuilder webAppFeatures = app
                                     .UseFeaturesFromAssemblies(typeof(ApiAssemblyMarker).Assembly)
                                     .Use<CrossCuttingConcerns>();

app.UseApiAdapter();

webAppFeatures.Use<ProductsFeature>();

app.Run();

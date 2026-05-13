using System.Reflection;

namespace Company.Template.Api.Endpoints;

internal static class EndpointModuleExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapEndpointModulesFromAssembly<TAssemblyMarker>()
        {
            return MapEndpointModulesFromAssemblies(app, typeof(TAssemblyMarker).Assembly);
        }
    }

    private static IEndpointRouteBuilder MapEndpointModulesFromAssemblies(
        IEndpointRouteBuilder app,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
        {
            throw new ArgumentException(
                "At least one assembly is required.",
                nameof(assemblies));
        }

        IEnumerable<IEndpointModule> modules = assemblies
                                              .Distinct()
                                              .SelectMany(assembly => assembly.DefinedTypes)
                                              .Where(type => type is { IsAbstract: false, IsInterface: false })
                                              .Where(type => typeof(IEndpointModule).IsAssignableFrom(type))
                                              .OrderBy(type => type.FullName, StringComparer.Ordinal)
                                              .Select(type => (IEndpointModule)Activator.CreateInstance(type.AsType())!);

        foreach (IEndpointModule module in modules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}

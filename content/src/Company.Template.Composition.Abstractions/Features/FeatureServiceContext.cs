using System.Reflection;

namespace Company.Template.Composition.Abstractions.Features;

/// <summary>
///     Provides feature service modules with the composition state needed to register their services.
/// </summary>
/// <remarks>
///     Configuration is optional so simple modules are not forced to depend on configuration. Modules that require it
///     should call <see cref="RequireConfiguration" /> to fail fast with a clear composition error.
/// </remarks>
public sealed class FeatureServiceContext
{
    public FeatureServiceContext(
        IServiceCollection services,
        IReadOnlyList<Assembly> assemblies,
        IConfiguration? configuration)
    {
        Services = services;
        Assemblies = assemblies;
        Configuration = configuration;
    }

    public IServiceCollection Services { get; }

    public IReadOnlyList<Assembly> Assemblies { get; }

    public IConfiguration? Configuration { get; }

    public IConfiguration RequireConfiguration()
    {
        return Configuration
            ?? throw new InvalidOperationException(
                "This feature service module requires configuration. Call WithConfiguration(...) before adding the feature.");
    }
}

using System.Reflection;

namespace Company.Template.Composition.Framework;

/// <summary>
///     Provides a fluent registration API for activating feature service modules from configured assemblies.
/// </summary>
/// <remarks>
///     The builder discovers modules only for explicitly selected features. Each module remains responsible for choosing
///     whether to register services explicitly or use local Scrutor conventions.
/// </remarks>
public sealed class FeatureServiceBuilder
{
    private readonly IReadOnlyList<Assembly> _assemblies;
    private readonly IServiceCollection _services;
    private IConfiguration? _configuration;

    internal FeatureServiceBuilder(
        IServiceCollection services,
        IReadOnlyList<Assembly> assemblies)
    {
        _services = services;
        _assemblies = assemblies;
    }

    public FeatureServiceBuilder WithConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuration = configuration;

        return this;
    }

    public FeatureServiceBuilder Add<TFeature>()
        where TFeature : IFeature
    {
        FeatureServiceContext context = new(
            _services,
            _assemblies,
            _configuration);

        IReadOnlyList<IFeatureServiceModule<TFeature>> modules =
            FeatureModuleDiscovery.CreateModules<IFeatureServiceModule<TFeature>>(_assemblies);

        foreach (IFeatureServiceModule<TFeature> module in modules)
        {
            module.Register(context);
        }

        return this;
    }
}

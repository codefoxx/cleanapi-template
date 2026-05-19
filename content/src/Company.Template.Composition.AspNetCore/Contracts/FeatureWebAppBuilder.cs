using System.Reflection;
using Company.Template.Composition.Abstractions.Contracts;
using Company.Template.Composition.AspNetCore.Contexts;
using Microsoft.AspNetCore.Builder;

namespace Company.Template.Composition.AspNetCore.Contracts;

/// <summary>
///     Provides a fluent API for activating feature WebApplication modules from configured assemblies.
/// </summary>
/// <remarks>
///     The builder discovers modules only for explicitly selected features. Web app modules can map endpoints,
///     register middleware, or apply other HTTP adapter pipeline configuration.
/// </remarks>
public sealed class FeatureWebAppBuilder
{
    private readonly WebApplication _app;
    private readonly IReadOnlyList<Assembly> _assemblies;

    internal FeatureWebAppBuilder(
        WebApplication app,
        IReadOnlyList<Assembly> assemblies)
    {
        _app = app;
        _assemblies = assemblies;
    }

    public FeatureWebAppBuilder Use<TFeature>()
        where TFeature : IFeature
    {
        FeatureWebAppContext context = new(
            _app,
            _assemblies);

        IReadOnlyList<IFeatureWebAppModule<TFeature>> modules =
            FeatureModuleDiscovery.CreateModules<IFeatureWebAppModule<TFeature>>(_assemblies);

        foreach (IFeatureWebAppModule<TFeature> module in modules)
        {
            module.Use(context);
        }

        return this;
    }
}

using System.Reflection;
using Company.Template.Composition.Abstractions.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Composition.Abstractions.Contracts;

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
    private readonly List<Type> _decoratorFeatures = [];
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

    internal void QueueDecorator<TDecoratorFeature>()
        where TDecoratorFeature : IFeature
    {
        Type decoratorFeature = typeof(TDecoratorFeature);

        if (_decoratorFeatures.Contains(decoratorFeature))
        {
            throw new InvalidOperationException(
                $"Decorator feature '{decoratorFeature.FullName}' was queued more than once in this composition scope. " +
                "Queue each decorator feature once, or consolidate the composition extension that applies it.");
        }

        _decoratorFeatures.Add(decoratorFeature);
    }

    internal void ApplyQueuedDecorators()
    {
        if (_decoratorFeatures.Count == 0)
        {
            return;
        }

        FeatureServiceContext context = new(
            _services,
            _assemblies,
            _configuration);

        foreach (Type decoratorFeature in _decoratorFeatures)
        {
            IReadOnlyList<FeatureServiceDecoratorModule> modules =
                FeatureModuleDiscovery.CreateServiceDecoratorModules(_assemblies, decoratorFeature);

            if (modules.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No service decorator modules were found for decorator feature '{decoratorFeature.FullName}'. " +
                    "Ensure a module implements IFeatureServiceDecoratorModule<TDecoratedFeature, TDecoratorFeature> " +
                    "for this decorator feature and that its assembly is included in AddFeatureServicesFromAssemblies(...).");
            }

            foreach (FeatureServiceDecoratorModule module in modules)
            {
                module.Decorate(context);
            }
        }

        _decoratorFeatures.Clear();
    }
}

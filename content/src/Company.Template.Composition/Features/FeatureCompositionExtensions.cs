using System.Reflection;

namespace Company.Template.Composition.Features;

/// <summary>
///     Provides fluent entry points for feature-oriented service and WebApplication composition.
/// </summary>
public static class FeatureCompositionExtensions
{
    extension(IServiceCollection services)
    {
        public FeatureServiceBuilder AddFeatureServicesFromAssemblies(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblies);

            if (assemblies.Length == 0)
            {
                throw new ArgumentException(
                    "At least one assembly is required.",
                    nameof(assemblies));
            }

            return new FeatureServiceBuilder(services, assemblies);
        }
    }

    extension(WebApplication app)
    {
        public FeatureWebAppBuilder UseFeaturesFromAssemblies(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(assemblies);

            if (assemblies.Length == 0)
            {
                throw new ArgumentException(
                    "At least one assembly is required.",
                    nameof(assemblies));
            }

            return new FeatureWebAppBuilder(app, assemblies);
        }
    }
}

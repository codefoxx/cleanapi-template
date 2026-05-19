using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Composition.Framework;

/// <summary>
///     Provides fluent entry points for feature-oriented service composition.
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

    extension(FeatureServiceBuilder builder)
    {
        public FeatureServiceBuilder ComposeFeatures(Action<FeatureCompositionContext> compose)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(compose);

            FeatureCompositionContext context = new(builder);

            compose(context);

            builder.ApplyQueuedDecorators();

            return builder;
        }
    }
}

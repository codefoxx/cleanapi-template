using System.Reflection;
using Company.Template.Composition.Abstractions.Contexts;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToExtensionBlock - prevents CS8620 errors

namespace Company.Template.Composition.Abstractions.Contracts;

/// <summary>
///     Provides fluent entry points for feature-oriented service composition.
/// </summary>
public static class FeatureCompositionExtensions
{
    public static FeatureServiceBuilder AddFeatureServicesFromAssemblies(this IServiceCollection services,
        Type assemblyMarker)
    {
        ArgumentNullException.ThrowIfNull(assemblyMarker);

        return services.AddFeatureServicesFromAssemblies([assemblyMarker]);
    }

    public static FeatureServiceBuilder AddFeatureServicesFromAssemblies(this IServiceCollection services,
        params Type[] assemblyMarkers)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblyMarkers);

        if (assemblyMarkers.Length == 0)
        {
            throw new ArgumentException(
                "At least one assembly is required.",
                nameof(assemblyMarkers));
        }

        Assembly[] assemblies = assemblyMarkers
            .Select(marker =>
            {
                ArgumentNullException.ThrowIfNull(marker);

                return marker.Assembly;
            })
            .ToArray();

        return new FeatureServiceBuilder(services, assemblies);
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

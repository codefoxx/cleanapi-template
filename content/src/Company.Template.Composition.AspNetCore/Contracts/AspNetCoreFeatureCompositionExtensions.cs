using System.Reflection;
using Microsoft.AspNetCore.Builder;

// ReSharper disable ConvertToExtensionBlock - prevents CS8620 errors

namespace Company.Template.Composition.AspNetCore.Contracts;

/// <summary>
///     Provides fluent entry points for feature-oriented ASP.NET Core pipeline composition.
/// </summary>
public static class AspNetCoreFeatureCompositionExtensions
{
    public static FeatureWebAppBuilder UseFeaturesFromAssemblies(this WebApplication app, params Type[] assemblyMarkers)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(assemblyMarkers);

        if (assemblyMarkers.Length == 0)
        {
            throw new ArgumentException(
                "At least one assembly is required.",
                nameof(assemblyMarkers));
        }

        Assembly[] assemblies = assemblyMarkers.Select(marker => marker.Assembly).ToArray();

        return new FeatureWebAppBuilder(app, assemblies);
    }
}

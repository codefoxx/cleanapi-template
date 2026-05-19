using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Company.Template.Composition.AspNetCore.Contracts;

/// <summary>
///     Provides fluent entry points for feature-oriented ASP.NET Core pipeline composition.
/// </summary>
public static class AspNetCoreFeatureCompositionExtensions
{
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

using System.Reflection;

namespace Company.Template.Composition.Abstractions.Features;

/// <summary>
///     Provides feature web app modules with the composition state needed to extend the ASP.NET Core pipeline.
/// </summary>
/// <remarks>
///     The context exposes the full <see cref="WebApplication" /> because a feature may need to map endpoints,
///     register middleware, or apply other HTTP adapter pipeline configuration.
/// </remarks>
public sealed class FeatureWebAppContext
{
    public FeatureWebAppContext(
        WebApplication app,
        IReadOnlyList<Assembly> assemblies)
    {
        App = app;
        Assemblies = assemblies;
    }

    public WebApplication App { get; }

    public IReadOnlyList<Assembly> Assemblies { get; }
}

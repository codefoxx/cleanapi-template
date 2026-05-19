using Company.Template.Composition.Abstractions.Contracts;
using Company.Template.Composition.AspNetCore.Contexts;

namespace Company.Template.Composition.AspNetCore.Contracts;

/// <summary>
///     Defines WebApplication pipeline changes that belong to a specific feature.
/// </summary>
/// <remarks>
///     Web app modules are intentionally broader than endpoint modules so a feature can add route mapping, middleware,
///     or other ASP.NET Core pipeline configuration from the HTTP adapter side.
/// </remarks>
public interface IFeatureWebAppModule<TFeature>
    where TFeature : IFeature
{
    void Use(FeatureWebAppContext context);
}

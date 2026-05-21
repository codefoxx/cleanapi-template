using Company.Template.Composition.Abstractions.Contexts;

namespace Company.Template.Composition.Abstractions.Contracts;

/// <summary>
///     Defines service registrations that belong to a specific feature.
/// </summary>
/// <remarks>
///     Feature service modules keep the active feature list explicit in the composition root while allowing each layer
///     to register its own part of a feature without relying on global assembly-wide scanning.
/// </remarks>
public interface IFeatureServiceModule<TFeature>
    where TFeature : IFeature
{
    void Register(FeatureServiceContext context);
}

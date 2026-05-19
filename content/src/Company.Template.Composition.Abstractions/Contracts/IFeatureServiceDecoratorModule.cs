using Company.Template.Composition.Abstractions.Contexts;

namespace Company.Template.Composition.Abstractions.Contracts;

/// <summary>
///     Defines service decorations that are applied after normal feature service registrations complete.
/// </summary>
/// <remarks>
///     The decorated feature identifies the application surface being decorated. The decorator feature identifies the
///     concern being applied and is the marker selected from the composition root.
/// </remarks>
public interface IFeatureServiceDecoratorModule<TDecoratedFeature, TDecoratorFeature>
    where TDecoratedFeature : IFeature
    where TDecoratorFeature : IFeature
{
    void Decorate(FeatureServiceContext context);
}

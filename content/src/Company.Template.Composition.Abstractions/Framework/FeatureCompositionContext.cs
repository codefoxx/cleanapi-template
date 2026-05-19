namespace Company.Template.Composition.Framework;

/// <summary>
///     Provides a scoped composition API for adding feature services before queued decorators are applied.
/// </summary>
public sealed class FeatureCompositionContext
{
    private readonly FeatureServiceBuilder _builder;

    internal FeatureCompositionContext(FeatureServiceBuilder builder)
    {
        _builder = builder;
    }

    public FeatureCompositionContext Add<TFeature>()
        where TFeature : IFeature
    {
        _builder.Add<TFeature>();

        return this;
    }

    public FeatureCompositionContext Decorate<TDecoratorFeature>()
        where TDecoratorFeature : IFeature
    {
        _builder.QueueDecorator<TDecoratorFeature>();

        return this;
    }
}

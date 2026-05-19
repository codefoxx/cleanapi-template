namespace Company.Template.Composition.Features;

/// <summary>
///     Defines the default technical features used by the generated template.
/// </summary>
public static class TemplateCompositionExtensions
{
    extension(FeatureCompositionContext context)
    {
        public FeatureCompositionContext AddTemplateDefaults()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context
                  .Add<PersistenceFeature>()
                  .Add<OpenApiFeature>()
                  .Add<DomainEventsFeature>()
                  .Add<CrossCuttingConcerns>();
        }

        public FeatureCompositionContext DecorateUseCasesWithTelemetry()
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Decorate<UseCaseTelemetryFeature>();
        }
    }
}

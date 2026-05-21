namespace Company.Template.CompositionRoot.Features;

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
                .Add<ApiAdapterFeature>()
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

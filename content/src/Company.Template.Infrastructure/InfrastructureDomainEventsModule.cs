using Company.Template.Application.Abstractions.DomainEvents;
using Company.Template.Composition.Features;
using Company.Template.Composition.Framework;
using Company.Template.Infrastructure.DomainEvents;

namespace Company.Template.Infrastructure;

/// <summary>
///     Registers the domain event dispatching adapter used by the application.
/// </summary>
/// <remarks>
///     The default dispatcher logs domain events. Real applications can replace this feature with an outbox or another
///     message-based dispatching mechanism.
/// </remarks>
public sealed class InfrastructureDomainEventsModule : IFeatureServiceModule<DomainEventsFeature>
{
    public void Register(FeatureServiceContext context)
    {
        context.Services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();
    }
}

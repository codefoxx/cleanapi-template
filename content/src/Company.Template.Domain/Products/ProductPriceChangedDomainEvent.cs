using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
/// Domain event recorded by the product aggregate after its price has changed.
/// Both the previous and new values are included so downstream reactions can observe the completed domain fact
/// without depending on aggregate internals.
/// </summary>
public sealed record ProductPriceChangedDomainEvent(
    ProductId ProductId,
    Money OldPrice,
    Money NewPrice,
    DateTimeOffset OccurredAt) : IDomainEvent;

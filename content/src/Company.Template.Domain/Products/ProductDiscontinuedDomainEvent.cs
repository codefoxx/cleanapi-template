using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
/// Domain event recorded by the product aggregate after it has been discontinued.
/// The event describes the lifecycle fact that occurred and leaves any reaction to application or infrastructure code.
/// </summary>
public sealed record ProductDiscontinuedDomainEvent(ProductId ProductId, DateTimeOffset OccurredOn) : IDomainEvent;

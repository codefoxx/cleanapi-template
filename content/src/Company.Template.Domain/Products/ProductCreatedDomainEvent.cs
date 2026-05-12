using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

/// <summary>
/// Domain event recorded by the product aggregate after a product has been created.
/// It records a fact that happened in the domain and decouples the aggregate from any reactions to that fact.
/// </summary>
public sealed record ProductCreatedDomainEvent(ProductId ProductId, DateTimeOffset OccurredOn) : IDomainEvent;

using System;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record ProductCreatedDomainEvent(ProductId ProductId, DateTimeOffset OccurredOn) : IDomainEvent;

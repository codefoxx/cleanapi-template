using System;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record ProductDiscontinuedDomainEvent(ProductId ProductId, DateTimeOffset OccurredOn) : IDomainEvent;

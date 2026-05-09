using System;
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record ProductPriceChangedDomainEvent(
    ProductId ProductId,
    Money OldPrice,
    Money NewPrice,
    DateTimeOffset OccurredOn) : IDomainEvent;

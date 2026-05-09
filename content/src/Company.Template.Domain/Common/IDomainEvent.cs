using System;

namespace Company.Template.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}

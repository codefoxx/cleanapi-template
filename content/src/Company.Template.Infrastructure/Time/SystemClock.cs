using Company.Template.Application.Abstractions.Time;

namespace Company.Template.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

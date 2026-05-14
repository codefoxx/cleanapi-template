using Company.Template.Application.Abstractions.Time;

namespace Company.Template.Application.Tests.TestSupport;

/// <summary>
///     Provides a deterministic clock for tests that need stable time-dependent assertions.
/// </summary>
/// <remarks>
///     A small test double is clearer than a mocking framework for this dependency because
///     tests only need to control the current time, not verify interactions with the clock.
/// </remarks>
public sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }
}

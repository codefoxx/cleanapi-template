namespace Company.Template.Application.Abstractions.Time;

/// <summary>
///     Provides an abstraction for the system clock.
/// </summary>
/// <remarks>
///     This abstraction is essential for unit testing domain logic that depends on
///     the current time, allowing tests to provide a fixed or controlled point in time.
/// </remarks>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

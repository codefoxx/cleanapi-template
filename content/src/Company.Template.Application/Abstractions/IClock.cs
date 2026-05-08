namespace Company.Template.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

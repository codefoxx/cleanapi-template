namespace Company.Template.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = DatabaseProvider.SelectedProvider;
}

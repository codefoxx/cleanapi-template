namespace Company.Template.Infrastructure.Options;

public sealed record DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = DatabaseProvider.SelectedProvider;
    public string ConnectionStringName { get; init; } = "DefaultConnection";
}

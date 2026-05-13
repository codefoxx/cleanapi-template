namespace Company.Template.Infrastructure.Options;

/// <summary>
///     Infrastructure configuration used to select the EF Core database provider and connection name.
///     The options belong to composition and persistence setup; they should not leak provider decisions into domain code.
/// </summary>
public sealed record DatabaseOptions
{
    public const string SectionName = "Database";
    public string ConnectionStringName { get; init; } = "DefaultConnection";

    public string Provider { get; init; } = DatabaseProvider.SelectedProvider;
}

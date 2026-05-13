namespace Company.Template.Infrastructure.Options;

/// <summary>
///     Represents the database provider selected when the template was generated.
/// </summary>
/// <remarks>
///     Provider selection is a template/build-time decision. Runtime configuration may confirm
///     the selected provider, but it cannot switch the generated application to another provider.
/// </remarks>
public static class DatabaseProvider
{
    public const string SelectedProvider = "__DB_PROVIDER__";

    public static bool MatchesSelectedProvider(string? provider)
    {
        return string.Equals(provider, SelectedProvider, StringComparison.OrdinalIgnoreCase);
    }
}

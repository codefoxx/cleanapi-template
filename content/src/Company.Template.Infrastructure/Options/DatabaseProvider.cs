using System;

namespace Company.Template.Infrastructure.Options;

public static class DatabaseProvider
{
    public const string SelectedProvider = "__DB_PROVIDER__";

    public static bool IsSupported(string provider)
    {
        return string.Equals(provider, SelectedProvider, StringComparison.OrdinalIgnoreCase);
    }
}

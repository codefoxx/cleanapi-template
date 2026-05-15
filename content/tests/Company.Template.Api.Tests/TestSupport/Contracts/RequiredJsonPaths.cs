namespace Company.Template.Api.Tests.TestSupport.Contracts;

internal sealed record RequiredJsonPaths(params string[] Paths)
{
    public static RequiredJsonPaths Empty => new();

    public RequiredJsonPaths And(params string[] paths)
    {
        return new RequiredJsonPaths([.. Paths, .. paths]);
    }
}

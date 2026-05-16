namespace Company.Template.TestSupport.Contracts;

public sealed record RequiredJsonPaths(params string[] Paths)
{
    public static RequiredJsonPaths Empty => new();

    public RequiredJsonPaths And(params string[] paths)
    {
        return new RequiredJsonPaths([.. Paths, .. paths]);
    }
}

namespace Company.Template.TestSupport.Contracts;

public static class ProblemJsonContracts
{
    public static readonly RequiredJsonPaths Problem = new(
        "$.title",
        "$.status",
        "$.code");

    public static readonly RequiredJsonPaths ValidationProblem = Problem.And(
        "$.errors.request");
}

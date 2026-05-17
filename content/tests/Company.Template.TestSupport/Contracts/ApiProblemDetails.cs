using System.Text.Json.Serialization;

namespace Company.Template.TestSupport.Contracts;

public sealed class ApiProblemDetails
{
    public string? Code { get; init; }

    public string? Detail { get; init; }

    public Dictionary<string, string[]>? Errors { get; init; }

    [JsonExtensionData] public Dictionary<string, JsonElement> Extensions { get; init; } = [];

    public int? Status { get; init; }

    public string? Title { get; init; }
    public string? Type { get; init; }
}

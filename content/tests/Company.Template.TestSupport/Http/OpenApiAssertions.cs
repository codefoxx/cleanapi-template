using System.Text.RegularExpressions;

namespace Company.Template.TestSupport.Http;

public static class OpenApiAssertions
{
    public static async Task<JsonDocument> GetOpenApiDocumentAsync(
        this HttpClient client)
    {
        string json = await client.GetStringAsync("/openapi/v1.json");

        return JsonDocument.Parse(json);
    }

    public static void ShouldAdvertiseResponse(
        this JsonDocument document,
        HttpMethod method,
        string path,
        int statusCode)
    {
        JsonElement paths = document.RootElement.GetProperty("paths");

        string? actualPath = paths
            .EnumerateObject()
            .Select(property => property.Name)
            .FirstOrDefault(openApiPath => NormalizePath(openApiPath) == path);

        actualPath.ShouldNotBeNull($"Expected OpenAPI path '{path}' to exist.");

        JsonElement pathItem = paths.GetProperty(actualPath);

        string operationName = method.Method.ToLowerInvariant();

        pathItem.TryGetProperty(operationName, out JsonElement operation)
                .ShouldBeTrue($"Expected OpenAPI path '{actualPath}' to advertise method '{operationName}'.");

        operation.TryGetProperty("responses", out JsonElement responses)
                 .ShouldBeTrue($"Expected OpenAPI operation '{operationName.ToUpperInvariant()} {actualPath}' to contain responses.");

        string statusCodeText = statusCode.ToString();

        responses.TryGetProperty(statusCodeText, out _)
                 .ShouldBeTrue($"Expected OpenAPI response '{statusCodeText}' for {method.Method} {actualPath}.");
    }

    private static string NormalizePath(string path)
    {
        return Regex.Replace(path, @"\{([^}:]+):[^}]+\}", "{$1}");
    }
}

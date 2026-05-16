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
        JsonElement responses = document.RootElement
                                        .GetProperty("paths")
                                        .GetProperty(path)
                                        .GetProperty(method.Method.ToLowerInvariant())
                                        .GetProperty("responses");

        responses.TryGetProperty(statusCode.ToString(), out _)
                 .ShouldBeTrue(
                      $"Expected OpenAPI response '{statusCode}' for {method.Method} {path}.");
    }
}

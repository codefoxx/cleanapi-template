using Company.Template.TestSupport.Contracts;

namespace Company.Template.TestSupport.Http;

public static class JsonContractAssertions
{
    public static async Task ShouldContainJsonPathsAsync(
        this HttpContent content,
        RequiredJsonPaths requiredPaths)
    {
        using JsonDocument document = JsonDocument.Parse(
            await content.ReadAsStringAsync());

        foreach (string path in requiredPaths.Paths)
        {
            document.RootElement.ShouldContainJsonPath(path);
        }
    }

    public static void ShouldContainJsonPath(
        this JsonElement root,
        string path)
    {
        string[] segments = path.TrimStart('$', '.')
                                .Split('.', StringSplitOptions.RemoveEmptyEntries);

        JsonElement current = root;

        foreach (string segment in segments)
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                current.GetArrayLength()
                       .ShouldBeGreaterThan(
                            0,
                            $"Expected array at '{path}' to contain at least one item.");

                current = current[0];
            }

            current.TryGetProperty(segment, out current)
                   .ShouldBeTrue($"Expected JSON path '{path}' to exist.");
        }
    }
}

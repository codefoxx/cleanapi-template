namespace Company.Template.TestSupport.Http;

public sealed record ApiEndpoint(HttpMethod Method, string Path)
{
    public static ApiEndpoint Get(string path)
    {
        return new ApiEndpoint(HttpMethod.Get, path);
    }

    public static ApiEndpoint Post(string path)
    {
        return new ApiEndpoint(HttpMethod.Post, path);
    }

    public static ApiEndpoint Put(string path)
    {
        return new ApiEndpoint(HttpMethod.Put, path);
    }

    public static ApiEndpoint Delete(string path)
    {
        return new ApiEndpoint(HttpMethod.Delete, path);
    }

    public Uri ToUri()
    {
        return new Uri(Path, UriKind.Relative);
    }
}

namespace Company.Template.Api.Endpoints;

/// <summary>
/// Defines an endpoint module that can register routes on the application's endpoint pipeline.
/// </summary>
/// <remarks>
/// Implement this interface in feature-specific endpoint classes to keep Program.cs small while still using standard
/// ASP.NET Core Minimal API route mapping.
/// </remarks>
internal interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app);
}

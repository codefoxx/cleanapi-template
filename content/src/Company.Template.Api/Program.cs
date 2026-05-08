using Company.Template.Api.CurrentUser;
using Company.Template.Api.Endpoints.Products;
using Company.Template.Api.Middleware;
using Company.Template.Api.OpenApi;
using Company.Template.Api.Security;
using Company.Template.Application;
using Company.Template.Application.Abstractions;
using Company.Template.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTemplateAuthentication(builder.Configuration);
builder.Services.AddTemplateAuthorization(builder.Configuration);
builder.Services.AddTemplateOpenApi();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var authenticationOptions = app.Services.GetRequiredService<Company.Template.Api.Options.AuthenticationOptions>();

if (authenticationOptions.Enabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Company.Template.Api",
    Status = "Running"
}));

app.MapProductEndpoints();

app.Run();

public partial class Program;

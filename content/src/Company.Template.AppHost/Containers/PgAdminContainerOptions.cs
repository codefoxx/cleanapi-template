namespace Company.Template.AppHost.Containers;

public sealed class PgAdminContainerOptions
{
    public string DefaultEmail { get; set; } = "admin@example.com";
    public string DefaultPassword { get; set; } = "admin";
    public string EndpointName { get; set; } = "http";
    public string Image { get; set; } = "dpage/pgadmin4";
    public string ResourceName { get; set; } = AppHostNames.PgAdminResourceName;
    public int TargetPort { get; set; } = 80;
}

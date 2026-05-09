# create-template.ps1
[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$TemplateRoot = (Get-Location).Path
$ContentRoot = Join-Path $TemplateRoot "content"
$PackageRoot = Join-Path $TemplateRoot "template-package"
$SolutionName = "Company.Template"
$SolutionFile = $null

function Write-Status {
    param([string]$Message)

    Write-Host $Message -ForegroundColor Cyan
}

function Write-File {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )

    $directory = Split-Path $Path -Parent

    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -Path $Path -Value $Content -Encoding UTF8
}

function Remove-GeneratedDefaultFiles {
    $files = @(
        "src/Company.Template.Domain/Class1.cs",
        "src/Company.Template.Application/Class1.cs",
        "src/Company.Template.Infrastructure/Class1.cs",
        "tests/Company.Template.Domain.Tests/UnitTest1.cs",
        "tests/Company.Template.Application.Tests/UnitTest1.cs",
        "tests/Company.Template.Infrastructure.Tests/UnitTest1.cs",
        "tests/Company.Template.Api.Tests/UnitTest1.cs",
        "src/Company.Template.Api/Program.cs"
    )

    foreach ($file in $files) {
        Remove-Item $file -ErrorAction SilentlyContinue
    }
}

function Resolve-SolutionFile {
    $slnx = "$SolutionName.slnx"
    $sln = "$SolutionName.sln"

    if (Test-Path $slnx) {
        return $slnx
    }

    if (Test-Path $sln) {
        return $sln
    }

    return $null
}

function Add-ProjectToSolution {
    param([string]$ProjectPath)

    if (-not $script:SolutionFile) {
        $script:SolutionFile = Resolve-SolutionFile
    }

    if (-not $script:SolutionFile) {
        throw "Could not find generated solution file for $SolutionName. Expected $SolutionName.slnx or $SolutionName.sln."
    }

    dotnet sln $script:SolutionFile add $ProjectPath
}

function Add-Reference {
    param(
        [string]$ProjectPath,
        [string]$ReferencePath
    )

    dotnet add $ProjectPath reference $ReferencePath
}

Write-Status "Preparing template repository folders..."

$generatedRoots = @($ContentRoot, $PackageRoot)
$existingGeneratedRoots = @($generatedRoots | Where-Object { Test-Path $_ })

if ($existingGeneratedRoots.Count -gt 0 -and -not $Force) {
    $existingList = $existingGeneratedRoots -join "', '"
    throw "Generated folders already exist: '$existingList'. Delete them first, run in an empty folder, or rerun with -Force."
}

if ($Force) {
    foreach ($generatedRoot in $generatedRoots) {
        if (Test-Path $generatedRoot) {
            Write-Status "Removing existing generated folder $generatedRoot..."
            Remove-Item $generatedRoot -Recurse -Force
        }
    }
}

New-Item -ItemType Directory -Path $ContentRoot -Force | Out-Null
New-Item -ItemType Directory -Path $PackageRoot -Force | Out-Null

Push-Location $ContentRoot
try {

Write-Status "Checking installed .NET SDKs..."
dotnet --list-sdks

$SolutionFile = Resolve-SolutionFile

if (-not $SolutionFile) {
    Write-Status "Creating solution $SolutionName..."
    dotnet new sln -n $SolutionName
    $SolutionFile = Resolve-SolutionFile
}

if (-not $SolutionFile) {
    throw "dotnet new sln did not create $SolutionName.slnx or $SolutionName.sln."
}

Write-Status "Using solution file $SolutionFile"

Write-Status "Creating folder structure..."
New-Item -ItemType Directory -Path "src" -Force | Out-Null
New-Item -ItemType Directory -Path "tests" -Force | Out-Null

Write-Status "Creating projects..."
dotnet new classlib -n Company.Template.Domain -o src/Company.Template.Domain --framework net10.0
dotnet new classlib -n Company.Template.Application -o src/Company.Template.Application --framework net10.0
dotnet new classlib -n Company.Template.Infrastructure -o src/Company.Template.Infrastructure --framework net10.0
dotnet new webapi -n Company.Template.Api -o src/Company.Template.Api --framework net10.0 --no-https
dotnet new aspire-servicedefaults -n Company.Template.ServiceDefaults -o src/Company.Template.ServiceDefaults --framework net10.0
dotnet new aspire-apphost -n Company.Template.AppHost -o src/Company.Template.AppHost --framework net10.0

dotnet new xunit -n Company.Template.Domain.Tests -o tests/Company.Template.Domain.Tests --framework net10.0
dotnet new xunit -n Company.Template.Application.Tests -o tests/Company.Template.Application.Tests --framework net10.0
dotnet new xunit -n Company.Template.Infrastructure.Tests -o tests/Company.Template.Infrastructure.Tests --framework net10.0
dotnet new xunit -n Company.Template.Api.Tests -o tests/Company.Template.Api.Tests --framework net10.0

Remove-GeneratedDefaultFiles

Write-Status "Adding projects to solution..."
Add-ProjectToSolution "src/Company.Template.Domain/Company.Template.Domain.csproj"
Add-ProjectToSolution "src/Company.Template.Application/Company.Template.Application.csproj"
Add-ProjectToSolution "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj"
Add-ProjectToSolution "src/Company.Template.Api/Company.Template.Api.csproj"
Add-ProjectToSolution "src/Company.Template.ServiceDefaults/Company.Template.ServiceDefaults.csproj"
Add-ProjectToSolution "src/Company.Template.AppHost/Company.Template.AppHost.csproj"
Add-ProjectToSolution "tests/Company.Template.Domain.Tests/Company.Template.Domain.Tests.csproj"
Add-ProjectToSolution "tests/Company.Template.Application.Tests/Company.Template.Application.Tests.csproj"
Add-ProjectToSolution "tests/Company.Template.Infrastructure.Tests/Company.Template.Infrastructure.Tests.csproj"
Add-ProjectToSolution "tests/Company.Template.Api.Tests/Company.Template.Api.Tests.csproj"

Write-Status "Adding project references..."
Add-Reference "src/Company.Template.Application/Company.Template.Application.csproj" "src/Company.Template.Domain/Company.Template.Domain.csproj"

Add-Reference "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj" "src/Company.Template.Application/Company.Template.Application.csproj"
Add-Reference "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj" "src/Company.Template.Domain/Company.Template.Domain.csproj"

Add-Reference "src/Company.Template.Api/Company.Template.Api.csproj" "src/Company.Template.Application/Company.Template.Application.csproj"
Add-Reference "src/Company.Template.Api/Company.Template.Api.csproj" "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj"
Add-Reference "src/Company.Template.Api/Company.Template.Api.csproj" "src/Company.Template.ServiceDefaults/Company.Template.ServiceDefaults.csproj"

Add-Reference "src/Company.Template.AppHost/Company.Template.AppHost.csproj" "src/Company.Template.Api/Company.Template.Api.csproj"
Add-Reference "src/Company.Template.AppHost/Company.Template.AppHost.csproj" "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj"
Add-Reference "src/Company.Template.AppHost/Company.Template.AppHost.csproj" "src/Company.Template.ServiceDefaults/Company.Template.ServiceDefaults.csproj"

Add-Reference "tests/Company.Template.Domain.Tests/Company.Template.Domain.Tests.csproj" "src/Company.Template.Domain/Company.Template.Domain.csproj"

Add-Reference "tests/Company.Template.Application.Tests/Company.Template.Application.Tests.csproj" "src/Company.Template.Application/Company.Template.Application.csproj"
Add-Reference "tests/Company.Template.Application.Tests/Company.Template.Application.Tests.csproj" "src/Company.Template.Domain/Company.Template.Domain.csproj"

Add-Reference "tests/Company.Template.Infrastructure.Tests/Company.Template.Infrastructure.Tests.csproj" "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj"
Add-Reference "tests/Company.Template.Infrastructure.Tests/Company.Template.Infrastructure.Tests.csproj" "src/Company.Template.Application/Company.Template.Application.csproj"
Add-Reference "tests/Company.Template.Infrastructure.Tests/Company.Template.Infrastructure.Tests.csproj" "src/Company.Template.Domain/Company.Template.Domain.csproj"

Add-Reference "tests/Company.Template.Api.Tests/Company.Template.Api.Tests.csproj" "src/Company.Template.Api/Company.Template.Api.csproj"
Add-Reference "tests/Company.Template.Api.Tests/Company.Template.Api.Tests.csproj" "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj"
Add-Reference "tests/Company.Template.Api.Tests/Company.Template.Api.Tests.csproj" "src/Company.Template.Application/Company.Template.Application.csproj"

Write-Status "Writing central build configuration..."

Write-File ".editorconfig" @'
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space
indent_size = 4

[*.{cs,csx}]
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

dotnet_style_qualification_for_event = false:suggestion
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_property = false:suggestion

dotnet_style_readonly_field = true:warning
dotnet_style_object_initializer = true:suggestion
dotnet_style_collection_initializer = true:suggestion
dotnet_style_prefer_collection_expression = true:suggestion

csharp_style_namespace_declarations = file_scoped:warning
csharp_prefer_braces = true:warning
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion
csharp_style_expression_bodied_methods = false:suggestion
csharp_style_expression_bodied_properties = when_on_single_line:suggestion
csharp_style_expression_bodied_accessors = when_on_single_line:suggestion
'@

Write-File ".gitignore" @'
bin/
obj/
.vs/
.idea/
*.user
*.suo
*.userosscache
*.sln.docstates
TestResults/
.coverage/
*.trx
.env
appsettings.Local.json
'@

Write-File "Directory.Build.props" @'
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Deterministic>true</Deterministic>
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <PropertyGroup Condition="$([System.String]::Copy('$(MSBuildProjectDirectory)').Contains('\src\'))">
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
'@

Write-File "Directory.Packages.props" @'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Aspire.Hosting.AppHost" Version="13.0.0" />
    <PackageVersion Include="Aspire.Hosting.PostgreSQL" Version="13.0.0" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageVersion Include="Aspire.Hosting.SqlServer" Version="13.0.0" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageVersion Include="Aspire.Hosting.MySql" Version="13.0.0" Condition="'__DB_PROVIDER__' == 'MySql'" />
    <PackageVersion Include="Aspire.Hosting.Keycloak" Version="13.0.0" />

    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Binder" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Options.DataAnnotations" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageVersion Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="10.0.0" />

    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageVersion Include="Pomelo.EntityFrameworkCore.MySql" Version="10.0.0" Condition="'__DB_PROVIDER__' == 'MySql'" />

    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageVersion Include="Serilog.Enrichers.Environment" Version="3.0.1" />
    <PackageVersion Include="Serilog.Enrichers.Thread" Version="4.0.0" />
    <PackageVersion Include="Serilog.Exceptions" Version="8.4.0" />
    <PackageVersion Include="Serilog.Settings.Configuration" Version="10.0.0" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="6.1.1" />

    <PackageVersion Include="Testcontainers.PostgreSql" Version="4.8.0" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageVersion Include="Testcontainers.MsSql" Version="4.8.0" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageVersion Include="Testcontainers.MySql" Version="4.8.0" Condition="'__DB_PROVIDER__' == 'MySql'" />

    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.0.0" />
    <PackageVersion Include="Shouldly" Version="4.3.0" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.1.5" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
  </ItemGroup>
</Project>
'@

Write-Status "Writing project files..."

Write-File "src/Company.Template.Domain/Company.Template.Domain.csproj" @'
<Project Sdk="Microsoft.NET.Sdk" />
'@

Write-File "src/Company.Template.Application/Company.Template.Application.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Company.Template.Domain\Company.Template.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
  </ItemGroup>

</Project>
'@

Write-File "src/Company.Template.Infrastructure/Company.Template.Infrastructure.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\Company.Template.Application\Company.Template.Application.csproj" />
    <ProjectReference Include="..\Company.Template.Domain\Company.Template.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="Persistence\Providers\PostgreSqlDatabaseProvider.cs" Condition="'__DB_PROVIDER__' != 'PostgreSql'" />
    <Compile Remove="Persistence\Providers\SqlServerDatabaseProvider.cs" Condition="'__DB_PROVIDER__' != 'SqlServer'" />
    <Compile Remove="Persistence\Providers\MySqlDatabaseProvider.cs" Condition="'__DB_PROVIDER__' != 'MySql'" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" />
    <PackageReference Include="Microsoft.Extensions.Options.DataAnnotations" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Condition="'__DB_PROVIDER__' == 'MySql'" />
  </ItemGroup>

</Project>
'@

Write-File "src/Company.Template.Api/Company.Template.Api.csproj" @'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <ItemGroup>
    <ProjectReference Include="..\Company.Template.Application\Company.Template.Application.csproj" />
    <ProjectReference Include="..\Company.Template.Infrastructure\Company.Template.Infrastructure.csproj" />
    <ProjectReference Include="..\Company.Template.ServiceDefaults\Company.Template.ServiceDefaults.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Binder" />
    <PackageReference Include="Serilog.AspNetCore" />
    <PackageReference Include="Serilog.Enrichers.Environment" />
    <PackageReference Include="Serilog.Enrichers.Thread" />
    <PackageReference Include="Serilog.Exceptions" />
    <PackageReference Include="Serilog.Settings.Configuration" />
    <PackageReference Include="Serilog.Sinks.Console" />
  </ItemGroup>

</Project>
'@

Write-File "src/Company.Template.AppHost/Company.Template.AppHost.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <Sdk Name="Aspire.AppHost.Sdk" Version="13.0.0" />

  <ItemGroup>
    <ProjectReference Include="..\Company.Template.Api\Company.Template.Api.csproj" />
    <ProjectReference Include="..\Company.Template.Infrastructure\Company.Template.Infrastructure.csproj" />
    <ProjectReference Include="..\Company.Template.ServiceDefaults\Company.Template.ServiceDefaults.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="Providers\PostgreSqlAspireDatabase.cs" Condition="'__DB_PROVIDER__' != 'PostgreSql'" />
    <Compile Remove="Providers\SqlServerAspireDatabase.cs" Condition="'__DB_PROVIDER__' != 'SqlServer'" />
    <Compile Remove="Providers\MySqlAspireDatabase.cs" Condition="'__DB_PROVIDER__' != 'MySql'" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" />
    <PackageReference Include="Aspire.Hosting.PostgreSQL" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageReference Include="Aspire.Hosting.SqlServer" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageReference Include="Aspire.Hosting.MySql" Condition="'__DB_PROVIDER__' == 'MySql'" />
    <PackageReference Include="Aspire.Hosting.Keycloak" />
  </ItemGroup>

</Project>
'@

Write-File "tests/Company.Template.Domain.Tests/Company.Template.Domain.Tests.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Company.Template.Domain\Company.Template.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
'@

Write-File "tests/Company.Template.Application.Tests/Company.Template.Application.Tests.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Company.Template.Application\Company.Template.Application.csproj" />
    <ProjectReference Include="..\..\src\Company.Template.Domain\Company.Template.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
'@

Write-File "tests/Company.Template.Infrastructure.Tests/Company.Template.Infrastructure.Tests.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Company.Template.Application\Company.Template.Application.csproj" />
    <ProjectReference Include="..\..\src\Company.Template.Domain\Company.Template.Domain.csproj" />
    <ProjectReference Include="..\..\src\Company.Template.Infrastructure\Company.Template.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="TestSupport\TestDatabase.PostgreSql.cs" Condition="'__DB_PROVIDER__' != 'PostgreSql'" />
    <Compile Remove="TestSupport\TestDatabase.SqlServer.cs" Condition="'__DB_PROVIDER__' != 'SqlServer'" />
    <Compile Remove="TestSupport\TestDatabase.MySql.cs" Condition="'__DB_PROVIDER__' != 'MySql'" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Testcontainers.PostgreSql" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageReference Include="Testcontainers.MsSql" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageReference Include="Testcontainers.MySql" Condition="'__DB_PROVIDER__' == 'MySql'" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
'@

Write-File "tests/Company.Template.Api.Tests/Company.Template.Api.Tests.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Company.Template.Api\Company.Template.Api.csproj" />
    <ProjectReference Include="..\..\src\Company.Template.Application\Company.Template.Application.csproj" />
    <ProjectReference Include="..\..\src\Company.Template.Infrastructure\Company.Template.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="TestSupport\TestDatabase.PostgreSql.cs" Condition="'__DB_PROVIDER__' != 'PostgreSql'" />
    <Compile Remove="TestSupport\TestDatabase.SqlServer.cs" Condition="'__DB_PROVIDER__' != 'SqlServer'" />
    <Compile Remove="TestSupport\TestDatabase.MySql.cs" Condition="'__DB_PROVIDER__' != 'MySql'" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Testcontainers.PostgreSql" Condition="'__DB_PROVIDER__' == 'PostgreSql'" />
    <PackageReference Include="Testcontainers.MsSql" Condition="'__DB_PROVIDER__' == 'SqlServer'" />
    <PackageReference Include="Testcontainers.MySql" Condition="'__DB_PROVIDER__' == 'MySql'" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
'@

Write-Status "Writing Domain layer..."

Write-File "src/Company.Template.Domain/Common/IDomainEvent.cs" @'
namespace Company.Template.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
'@

Write-File "src/Company.Template.Domain/Common/AggregateRoot.cs" @'
namespace Company.Template.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
'@

Write-File "src/Company.Template.Domain/Products/ProductId.cs" @'
namespace Company.Template.Domain.Products;

public readonly record struct ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Product id cannot be empty.", nameof(value));
        }

        return new ProductId(value);
    }

    public override string ToString() => Value.ToString();
}
'@

Write-File "src/Company.Template.Domain/Products/ProductName.cs" @'
namespace Company.Template.Domain.Products;

public sealed record ProductName
{
    public const int MaxLength = 200;

    private ProductName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Product name is required.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Product name cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new ProductName(trimmed);
    }

    public override string ToString() => Value;
}
'@

Write-File "src/Company.Template.Domain/Products/Money.cs" @'
namespace Company.Template.Domain.Products;

public sealed record Money
{
    public const int CurrencyMaxLength = 3;

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != CurrencyMaxLength)
        {
            throw new ArgumentException("Currency must be an ISO 4217 three-letter code.", nameof(currency));
        }

        return new Money(amount, normalizedCurrency);
    }

    public static Money Zero(string currency) => Create(0, currency);
}
'@

Write-File "src/Company.Template.Domain/Products/ProductStatus.cs" @'
namespace Company.Template.Domain.Products;

public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Discontinued = 2
}
'@

Write-File "src/Company.Template.Domain/Products/ProductCreatedDomainEvent.cs" @'
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record ProductCreatedDomainEvent(ProductId ProductId, DateTimeOffset OccurredOn) : IDomainEvent;
'@

Write-File "src/Company.Template.Domain/Products/ProductPriceChangedDomainEvent.cs" @'
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record ProductPriceChangedDomainEvent(
    ProductId ProductId,
    Money OldPrice,
    Money NewPrice,
    DateTimeOffset OccurredOn) : IDomainEvent;
'@

Write-File "src/Company.Template.Domain/Products/ProductDiscontinuedDomainEvent.cs" @'
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed record ProductDiscontinuedDomainEvent(ProductId ProductId, DateTimeOffset OccurredOn) : IDomainEvent;
'@

Write-File "src/Company.Template.Domain/Products/Product.cs" @'
using Company.Template.Domain.Common;

namespace Company.Template.Domain.Products;

public sealed class Product : AggregateRoot
{
    private Product()
    {
        Name = null!;
        Price = null!;
    }

    private Product(ProductId id, ProductName name, Money price, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Price = price;
        Status = ProductStatus.Active;
        CreatedAt = createdAt;
    }

    public ProductId Id { get; private set; }

    public ProductName Name { get; private set; }

    public Money Price { get; private set; }

    public ProductStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? DiscontinuedAt { get; private set; }

    public static Product Create(ProductName name, Money price, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(price);

        var product = new Product(ProductId.New(), name, price, createdAt);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, createdAt));

        return product;
    }

    public void Rename(ProductName newName)
    {
        ArgumentNullException.ThrowIfNull(newName);

        if (Status == ProductStatus.Discontinued)
        {
            throw new InvalidOperationException("A discontinued product cannot be renamed.");
        }

        if (Name == newName)
        {
            return;
        }

        Name = newName;
    }

    public void ChangePrice(Money newPrice, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (Price == newPrice)
        {
            return;
        }

        var oldPrice = Price;
        Price = newPrice;

        AddDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice, changedAt));
    }

    public void Discontinue(DateTimeOffset discontinuedAt)
    {
        if (Status == ProductStatus.Discontinued)
        {
            return;
        }

        Status = ProductStatus.Discontinued;
        DiscontinuedAt = discontinuedAt;

        AddDomainEvent(new ProductDiscontinuedDomainEvent(Id, discontinuedAt));
    }
}
'@

Write-Status "Writing Application layer..."

Write-File "src/Company.Template.Application/Abstractions/IClock.cs" @'
namespace Company.Template.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
'@

Write-File "src/Company.Template.Application/Abstractions/ICurrentUser.cs" @'
namespace Company.Template.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
'@

Write-File "src/Company.Template.Application/Abstractions/IDomainEventDispatcher.cs" @'
using Company.Template.Domain.Common;

namespace Company.Template.Application.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
'@

Write-File "src/Company.Template.Application/Abstractions/IProductRepository.cs" @'
using Company.Template.Domain.Products;

namespace Company.Template.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);
}
'@

Write-File "src/Company.Template.Application/Abstractions/IUnitOfWork.cs" @'
namespace Company.Template.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
'@

Write-File "src/Company.Template.Application/Common/Result.cs" @'
namespace Company.Template.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static Error NotFound(string message) => new("not_found", message);

    public static Error Validation(string message) => new("validation_error", message);

    public static Error Conflict(string message) => new("conflict", message);
}

public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error) => new(default, error);
}

public sealed class Result
{
    private Result(Error? error)
    {
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);
}
'@

Write-File "src/Company.Template.Application/Products/ProductDto.cs" @'
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    ProductStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
'@

Write-File "src/Company.Template.Application/Products/ProductMapper.cs" @'
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products;

internal static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto(
            product.Id.Value,
            product.Name.Value,
            product.Price.Amount,
            product.Price.Currency,
            product.Status,
            product.CreatedAt,
            product.DiscontinuedAt);
    }
}
'@

Write-File "src/Company.Template.Application/Products/CreateProduct/CreateProductCommand.cs" @'
namespace Company.Template.Application.Products.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal Price, string Currency);
'@

Write-File "src/Company.Template.Application/Products/CreateProduct/CreateProductUseCase.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.CreateProduct;

public sealed class CreateProductUseCase
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateProductUseCase(IProductRepository products, IUnitOfWork unitOfWork, IClock clock)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<ProductDto>.Failure(Error.Validation("Product name is required."));
        }

        if (command.Price < 0)
        {
            return Result<ProductDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        Product product;

        try
        {
            product = Product.Create(
                ProductName.Create(command.Name),
                Money.Create(command.Price, command.Currency),
                _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return Result<ProductDto>.Failure(Error.Validation(exception.Message));
        }

        await _products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
'@

Write-File "src/Company.Template.Application/Products/GetProductById/GetProductByIdQuery.cs" @'
namespace Company.Template.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId);
'@

Write-File "src/Company.Template.Application/Products/GetProductById/GetProductByIdUseCase.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.GetProductById;

public sealed class GetProductByIdUseCase
{
    private readonly IProductRepository _products;

    public GetProductByIdUseCase(IProductRepository products)
    {
        _products = products;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        var product = await _products.GetByIdAsync(ProductId.From(query.ProductId), cancellationToken);

        return product is null
            ? Result<ProductDto>.Failure(Error.NotFound("Product was not found."))
            : Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
'@

Write-File "src/Company.Template.Application/Products/ChangeProductPrice/ChangeProductPriceCommand.cs" @'
namespace Company.Template.Application.Products.ChangeProductPrice;

public sealed record ChangeProductPriceCommand(Guid ProductId, decimal Price, string Currency);
'@

Write-File "src/Company.Template.Application/Products/ChangeProductPrice/ChangeProductPriceUseCase.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.ChangeProductPrice;

public sealed class ChangeProductPriceUseCase
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ChangeProductPriceUseCase(IProductRepository products, IUnitOfWork unitOfWork, IClock clock)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(ChangeProductPriceCommand command, CancellationToken cancellationToken)
    {
        if (command.ProductId == Guid.Empty)
        {
            return Result<ProductDto>.Failure(Error.Validation("Product id is required."));
        }

        if (command.Price < 0)
        {
            return Result<ProductDto>.Failure(Error.Validation("Price cannot be negative."));
        }

        var product = await _products.GetByIdAsync(ProductId.From(command.ProductId), cancellationToken);

        if (product is null)
        {
            return Result<ProductDto>.Failure(Error.NotFound("Product was not found."));
        }

        try
        {
            product.ChangePrice(Money.Create(command.Price, command.Currency), _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return Result<ProductDto>.Failure(Error.Validation(exception.Message));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(ProductMapper.ToDto(product));
    }
}
'@

Write-File "src/Company.Template.Application/Products/DiscontinueProduct/DiscontinueProductCommand.cs" @'
namespace Company.Template.Application.Products.DiscontinueProduct;

public sealed record DiscontinueProductCommand(Guid ProductId);
'@

Write-File "src/Company.Template.Application/Products/DiscontinueProduct/DiscontinueProductUseCase.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Application.Common;
using Company.Template.Domain.Products;

namespace Company.Template.Application.Products.DiscontinueProduct;

public sealed class DiscontinueProductUseCase
{
    private readonly IProductRepository _products;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public DiscontinueProductUseCase(IProductRepository products, IUnitOfWork unitOfWork, IClock clock)
    {
        _products = products;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> ExecuteAsync(DiscontinueProductCommand command, CancellationToken cancellationToken)
    {
        if (command.ProductId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Product id is required."));
        }

        var product = await _products.GetByIdAsync(ProductId.From(command.ProductId), cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Product was not found."));
        }

        product.Discontinue(_clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
'@

Write-File "src/Company.Template.Application/DependencyInjection.cs" @'
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<GetProductByIdUseCase>();
        services.AddScoped<ChangeProductPriceUseCase>();
        services.AddScoped<DiscontinueProductUseCase>();

        return services;
    }
}
'@

Write-Status "Writing Infrastructure layer..."

Write-File "src/Company.Template.Infrastructure/Options/DatabaseOptions.cs" @'
namespace Company.Template.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = DatabaseProvider.SelectedProvider;
}
'@

Write-File "src/Company.Template.Infrastructure/Options/DatabaseProvider.cs" @'
namespace Company.Template.Infrastructure.Options;

public static class DatabaseProvider
{
    public const string SelectedProvider = "__DB_PROVIDER__";

    public static bool IsSupported(string provider)
    {
        return string.Equals(provider, SelectedProvider, StringComparison.OrdinalIgnoreCase);
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Time/SystemClock.cs" @'
using Company.Template.Application.Abstractions;

namespace Company.Template.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
'@

Write-File "src/Company.Template.Infrastructure/DomainEvents/LoggingDomainEventDispatcher.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Company.Template.Infrastructure.DomainEvents;

internal sealed class LoggingDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<LoggingDomainEventDispatcher> _logger;

    public LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Domain event dispatched: {DomainEvent}", domainEvent.GetType().Name);
        }

        return Task.CompletedTask;
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/ApplicationDbContext.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventDispatcher? domainEventDispatcher = null)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<Product> Products => Set<Product>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null && domainEvents.Length > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            {
                entry.Entity.ClearDomainEvents();
            }
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/Configurations/ProductConfiguration.cs" @'
using Company.Template.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Template.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .HasConversion(id => id.Value, value => ProductId.From(value))
            .ValueGeneratedNever();

        builder.OwnsOne(product => product.Name, name =>
        {
            name.Property(value => value.Value)
                .HasColumnName("name")
                .HasMaxLength(ProductName.MaxLength)
                .IsRequired();
        });

        builder.OwnsOne(product => product.Price, price =>
        {
            price.Property(value => value.Amount)
                .HasColumnName("price_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property(value => value.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(Money.CurrencyMaxLength)
                .IsRequired();
        });

        builder.Property(product => product.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .IsRequired();

        builder.Property(product => product.DiscontinuedAt);

        builder.Ignore(product => product.DomainEvents);
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/ProductRepository.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Infrastructure.Persistence;

internal sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken)
    {
        return _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/DatabaseRegistrationExtensions.cs" @'
using Company.Template.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure.Persistence;

public static class DatabaseRegistrationExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        if (!DatabaseProvider.IsSupported(databaseOptions.Provider))
        {
            throw new InvalidOperationException(
                $"Unsupported database provider '{databaseOptions.Provider}'. This template was generated for '{DatabaseProvider.SelectedProvider}'.");
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
        }

        return SelectedDatabaseProvider.AddDatabase(services, connectionString);
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/Providers/PostgreSqlDatabaseProvider.cs" @'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure.Persistence;

internal static class SelectedDatabaseProvider
{
    public static IServiceCollection AddDatabase(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");

        return services;
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/Providers/SqlServerDatabaseProvider.cs" @'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure.Persistence;

internal static class SelectedDatabaseProvider
{
    public static IServiceCollection AddDatabase(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");

        return services;
    }
}
'@

Write-File "src/Company.Template.Infrastructure/Persistence/Providers/MySqlDatabaseProvider.cs" @'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure.Persistence;

internal static class SelectedDatabaseProvider
{
    public static IServiceCollection AddDatabase(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");

        return services;
    }
}
'@

Write-File "src/Company.Template.Infrastructure/DependencyInjection.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Infrastructure.DomainEvents;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, LoggingDomainEventDispatcher>();

        return services;
    }
}
'@

Write-Status "Writing API layer..."

Write-File "src/Company.Template.Api/Options/AuthenticationOptions.cs" @'
namespace Company.Template.Api.Options;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public bool Enabled { get; init; }

    public string Authority { get; init; } = "";

    public string Audience { get; init; } = "company-template-api";

    public bool RequireHttpsMetadata { get; init; }

    public string RoleClaimType { get; init; } = "roles";
}
'@

Write-File "src/Company.Template.Api/Security/TemplatePolicies.cs" @'
namespace Company.Template.Api.Security;

public static class TemplatePolicies
{
    public const string ProductsRead = "products.read";

    public const string ProductsWrite = "products.write";
}
'@

Write-File "src/Company.Template.Api/Security/AuthenticationExtensions.cs" @'
using Company.Template.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Company.Template.Api.Security;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddTemplateAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        services.AddSingleton(options);

        if (!options.Enabled)
        {
            return services;
        }

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            throw new InvalidOperationException("Authentication:Authority is required when authentication is enabled.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Authentication:Audience is required when authentication is enabled.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwt.TokenValidationParameters.RoleClaimType = options.RoleClaimType;
            });

        return services;
    }

    public static IServiceCollection AddTemplateAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

        if (!options.Enabled)
        {
            services.AddAuthorization();
            return services;
        }

        services.AddAuthorization(authorization =>
        {
            authorization.AddPolicy(TemplatePolicies.ProductsRead, policy => RequireScopeOrRole(policy, TemplatePolicies.ProductsRead));
            authorization.AddPolicy(TemplatePolicies.ProductsWrite, policy => RequireScopeOrRole(policy, TemplatePolicies.ProductsWrite));
        });

        return services;
    }

    public static RouteHandlerBuilder RequireTemplatePolicy(
        this RouteHandlerBuilder builder,
        string policy,
        AuthenticationOptions authenticationOptions)
    {
        return authenticationOptions.Enabled
            ? builder.RequireAuthorization(policy)
            : builder;
    }

    private static void RequireScopeOrRole(AuthorizationPolicyBuilder policy, string requiredValue)
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.IsInRole(requiredValue) ||
            context.User.Claims.Any(claim =>
                (claim.Type is "scope" or "scp" && claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(requiredValue)) ||
                claim.Value == requiredValue));
    }
}
'@

Write-File "src/Company.Template.Api/CurrentUser/HttpCurrentUser.cs" @'
using System.Security.Claims;
using Company.Template.Application.Abstractions;

namespace Company.Template.Api.CurrentUser;

internal sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
    }
}
'@

Write-File "src/Company.Template.Api/Endpoints/EndpointResultExtensions.cs" @'
using Company.Template.Application.Common;

namespace Company.Template.Api.Endpoints;

internal static class EndpointResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return onSuccess(result.Value);
        }

        return ToProblem(result.Error);
    }

    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error? error)
    {
        if (error is null)
        {
            return Results.Problem(title: "Unexpected error.");
        }

        return error.Code switch
        {
            "validation_error" => Results.ValidationProblem(
                new Dictionary<string, string[]> { ["request"] = [error.Message] },
                title: "Validation failed."),

            "not_found" => Results.Problem(
                title: "Resource not found.",
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound),

            "conflict" => Results.Problem(
                title: "Conflict.",
                detail: error.Message,
                statusCode: StatusCodes.Status409Conflict),

            _ => Results.Problem(
                title: "Request failed.",
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
'@

Write-File "src/Company.Template.Api/Endpoints/Products/ProductContracts.cs" @'
namespace Company.Template.Api.Endpoints.Products;

public sealed record CreateProductRequest(string Name, decimal Price, string Currency);

public sealed record ChangeProductPriceRequest(decimal Price, string Currency);

public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DiscontinuedAt);
'@

Write-File "src/Company.Template.Api/Endpoints/Products/ProductEndpointMapper.cs" @'
using Company.Template.Application.Products;

namespace Company.Template.Api.Endpoints.Products;

internal static class ProductEndpointMapper
{
    public static ProductResponse ToResponse(ProductDto product)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Price,
            product.Currency,
            product.Status.ToString(),
            product.CreatedAt,
            product.DiscontinuedAt);
    }
}
'@

Write-File "src/Company.Template.Api/Endpoints/Products/ProductEndpoints.cs" @'
using Company.Template.Api.Options;
using Company.Template.Api.Security;
using Company.Template.Application.Products.ChangeProductPrice;
using Company.Template.Application.Products.CreateProduct;
using Company.Template.Application.Products.DiscontinueProduct;
using Company.Template.Application.Products.GetProductById;

namespace Company.Template.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var authenticationOptions = app.ServiceProvider.GetRequiredService<AuthenticationOptions>();

        var group = app
            .MapGroup("/api/products")
            .WithTags("Products");

        group
            .MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions);

        group
            .MapGet("/{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsRead, authenticationOptions);

        group
            .MapPut("/{id:guid}/price", ChangeProductPriceAsync)
            .WithName("ChangeProductPrice")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions);

        group
            .MapPost("/{id:guid}/discontinue", DiscontinueProductAsync)
            .WithName("DiscontinueProduct")
            .WithOpenApi()
            .RequireTemplatePolicy(TemplatePolicies.ProductsWrite, authenticationOptions);

        return app;
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        CreateProductUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new CreateProductCommand(request.Name, request.Price, request.Currency),
            cancellationToken);

        return result.ToHttpResult(product =>
            Results.Created($"/api/products/{product.Id}", ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        GetProductByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetProductByIdQuery(id), cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> ChangeProductPriceAsync(
        Guid id,
        ChangeProductPriceRequest request,
        ChangeProductPriceUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(
            new ChangeProductPriceCommand(id, request.Price, request.Currency),
            cancellationToken);

        return result.ToHttpResult(product => Results.Ok(ProductEndpointMapper.ToResponse(product)));
    }

    private static async Task<IResult> DiscontinueProductAsync(
        Guid id,
        DiscontinueProductUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new DiscontinueProductCommand(id), cancellationToken);

        return result.ToHttpResult();
    }
}
'@

Write-File "src/Company.Template.Api/OpenApi/OpenApiExtensions.cs" @'
using Microsoft.OpenApi.Models;

namespace Company.Template.Api.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddTemplateOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT bearer token"
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
'@

Write-File "src/Company.Template.Api/Middleware/GlobalExceptionHandler.cs" @'
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Company.Template.Api.Middleware;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred.");

        var problem = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Detail = "The server encountered an unexpected condition.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
'@

Write-File "src/Company.Template.Api/Program.cs" @'
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
'@

Write-File "src/Company.Template.Api/appsettings.json" @'
{
  "Database": {
    "Provider": "__DB_PROVIDER__"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Set by Aspire AppHost. Replace this when running the API directly."
  },
  "Authentication": {
    "Enabled": false,
    "Authority": "",
    "Audience": "company-template-api",
    "RequireHttpsMetadata": false,
    "RoleClaimType": "roles"
  },
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Enrichers.Environment",
      "Serilog.Enrichers.Thread",
      "Serilog.Exceptions"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId",
      "WithExceptionDetails"
    ],
    "WriteTo": [
      { "Name": "Console" }
    ]
  },
  "AllowedHosts": "*"
}
'@

Write-File "src/Company.Template.Api/appsettings.Development.json" @'
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Company.Template": "Debug",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
'@

Write-File "src/Company.Template.Api/Properties/launchSettings.json" @'
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "Company.Template.Api": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "openapi/v1.json",
      "applicationUrl": "http://localhost:5080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
'@

Write-Status "Writing Aspire AppHost..."

Write-File "src/Company.Template.AppHost/Program.cs" @'
using Aspire.Hosting.ApplicationModel;
using Company.Template.AppHost.Providers;

var builder = DistributedApplication.CreateBuilder(args);

const string databaseProvider = "__DB_PROVIDER__";
const bool enableKeycloak = false;

IResourceBuilder<IResourceWithConnectionString> database = AspireDatabase.Create(builder);

var api = builder
    .AddProject<Projects.Company_Template_Api>("company-template-api")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Database__Provider", databaseProvider);

if (enableKeycloak)
{
    var keycloak = builder
        .AddKeycloak("keycloak", 8080)
        .WithDataVolume();

    api
        .WithReference(keycloak)
        .WaitFor(keycloak)
        .WithEnvironment("Authentication__Enabled", "true")
        .WithEnvironment("Authentication__Authority", "http://localhost:8080/realms/company-template")
        .WithEnvironment("Authentication__Audience", "company-template-api");
}

builder.Build().Run();
'@

Write-File "src/Company.Template.AppHost/Providers/PostgreSqlAspireDatabase.cs" @'
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Company.Template.AppHost.Providers;

internal static class AspireDatabase
{
    public static IResourceBuilder<IResourceWithConnectionString> Create(IDistributedApplicationBuilder builder)
    {
        return builder
            .AddPostgres("postgres")
            .AddDatabase("DefaultConnection");
    }
}
'@

Write-File "src/Company.Template.AppHost/Providers/SqlServerAspireDatabase.cs" @'
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Company.Template.AppHost.Providers;

internal static class AspireDatabase
{
    public static IResourceBuilder<IResourceWithConnectionString> Create(IDistributedApplicationBuilder builder)
    {
        return builder
            .AddSqlServer("sqlserver")
            .AddDatabase("DefaultConnection");
    }
}
'@

Write-File "src/Company.Template.AppHost/Providers/MySqlAspireDatabase.cs" @'
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Company.Template.AppHost.Providers;

internal static class AspireDatabase
{
    public static IResourceBuilder<IResourceWithConnectionString> Create(IDistributedApplicationBuilder builder)
    {
        return builder
            .AddMySql("mysql")
            .AddDatabase("DefaultConnection");
    }
}
'@

Write-File "tests/Company.Template.Infrastructure.Tests/TestSupport/TestDatabase.PostgreSql.cs" @'
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Company.Template.Infrastructure.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("company_template_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
    }
}
'@

Write-File "tests/Company.Template.Infrastructure.Tests/TestSupport/TestDatabase.SqlServer.cs" @'
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Company.Template.Infrastructure.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
    }
}
'@

Write-File "tests/Company.Template.Infrastructure.Tests/TestSupport/TestDatabase.MySql.cs" @'
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Company.Template.Infrastructure.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:9")
        .WithDatabase("company_template_tests")
        .WithUsername("mysql")
        .WithPassword("mysql")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        var connectionString = _container.GetConnectionString();

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;
    }
}
'@

Write-File "tests/Company.Template.Infrastructure.Tests/PersistenceTests.cs" @'
using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Domain.Products;
using Company.Template.Infrastructure.Persistence;
using Company.Template.Infrastructure.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Company.Template.Infrastructure.Tests;

public sealed class PersistenceTests : IClassFixture<TestDatabase>
{
    private readonly TestDatabase _database;

    public PersistenceTests(TestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task CanPersistAndLoadProduct()
    {
        await using var dbContext = new ApplicationDbContext(
            _database.CreateDbContextOptions(),
            new NoOpDomainEventDispatcher());

        await dbContext.Database.EnsureCreatedAsync();

        var product = Product.Create(
            ProductName.Create("Keyboard"),
            Money.Create(99.99m, "USD"),
            DateTimeOffset.UtcNow);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var loaded = await dbContext.Products.SingleAsync(entity => entity.Id == product.Id);

        loaded.Id.ShouldBe(product.Id);
        loaded.Name.Value.ShouldBe("Keyboard");
        loaded.Price.Amount.ShouldBe(99.99m);
        loaded.Price.Currency.ShouldBe("USD");
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
'@

Write-File "tests/Company.Template.Api.Tests/TestSupport/TestDatabase.PostgreSql.cs" @'
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Company.Template.Api.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("company_template_api_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public void Configure(DbContextOptionsBuilder<ApplicationDbContext> builder)
    {
        builder.UseNpgsql(_container.GetConnectionString());
    }
}
'@

Write-File "tests/Company.Template.Api.Tests/TestSupport/TestDatabase.SqlServer.cs" @'
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Company.Template.Api.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public void Configure(DbContextOptionsBuilder<ApplicationDbContext> builder)
    {
        builder.UseSqlServer(_container.GetConnectionString());
    }
}
'@

Write-File "tests/Company.Template.Api.Tests/TestSupport/TestDatabase.MySql.cs" @'
using Company.Template.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Company.Template.Api.Tests.TestSupport;

public sealed class TestDatabase : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:9")
        .WithDatabase("company_template_api_tests")
        .WithUsername("mysql")
        .WithPassword("mysql")
        .Build();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public void Configure(DbContextOptionsBuilder<ApplicationDbContext> builder)
    {
        var connectionString = _container.GetConnectionString();
        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}
'@

Write-File "tests/Company.Template.Api.Tests/ApiTestFactory.cs" @'
using Company.Template.Api.Tests.TestSupport;
using Company.Template.Application.Abstractions;
using Company.Template.Domain.Common;
using Company.Template.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestDatabase _database = new();

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();
            services.AddDbContext<ApplicationDbContext>(_database.Configure);
        });
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
'@

Write-File "tests/Company.Template.Api.Tests/ApiSmokeTests.cs" @'
using System.Net;
using System.Net.Http.Json;

namespace Company.Template.Api.Tests;

public sealed class ApiSmokeTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ApiSmokeTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_ReturnsOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_ReturnsProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            Name = "",
            Price = 10,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetProduct_ReturnsProduct()
    {
        using var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/products", new
        {
            Name = "Keyboard",
            Price = 99.99m,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/products/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    private sealed record ProductResponse(
        Guid Id,
        string Name,
        decimal Price,
        string Currency,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? DiscontinuedAt);
}
'@

Write-Status "Writing README..."

Write-File "README.md" @'
# Company.Template

Production-oriented Clean Architecture Web API generated from the `cleanapi` template.

## Project structure

~~~text
src/
  Company.Template.Api
  Company.Template.Application
  Company.Template.Domain
  Company.Template.Infrastructure
  Company.Template.ServiceDefaults
  Company.Template.AppHost

tests/
  Company.Template.Domain.Tests
  Company.Template.Application.Tests
  Company.Template.Infrastructure.Tests
  Company.Template.Api.Tests
~~~

## Architecture rules

- `Domain` references no other project.
- `Application` references `Domain` only.
- `Infrastructure` references `Application` and `Domain`.
- `Api` references `Application`, `Infrastructure`, and `ServiceDefaults`.
- `AppHost` is used for local orchestration with .NET Aspire.
- Tests reference only the projects they need.

The Domain layer must not reference EF Core, ASP.NET Core, Keycloak, Aspire, or any other infrastructure concern.

## Sample domain

The template includes a small Catalog/Product domain:

- `Product` aggregate root
- `ProductId` strongly typed ID
- `ProductName` value object
- `Money` value object
- `ProductStatus`
- domain events:
  - `ProductCreatedDomainEvent`
  - `ProductPriceChangedDomainEvent`
  - `ProductDiscontinuedDomainEvent`

Business rules live in the domain model:

- product name must not be empty
- price must not be negative
- discontinued products cannot be renamed
- changing price to the same value does nothing
- domain events are raised only when state changes

## Database provider selection

Configure the provider in `src/Company.Template.Api/appsettings.json`:

~~~json
{
  "Database": {
    "Provider": "__DB_PROVIDER__"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Set by Aspire AppHost. Replace this when running the API directly."
  }
}
~~~

Valid provider values:

- `PostgreSql`
- `SqlServer`
- `MySql`

Provider selection is centralized in:

~~~text
src/Company.Template.Infrastructure/Persistence/DatabaseRegistrationExtensions.cs
~~~

## Running with Aspire

Run:

~~~bash
dotnet run --project src/Company.Template.AppHost
~~~

Change the local provider in:

~~~text
src/Company.Template.AppHost/Program.cs
~~~

~~~csharp
const string databaseProvider = "__DB_PROVIDER__";
const bool enableKeycloak = false;
~~~

Aspire starts the selected database container and wires the connection string to the API.

## Optional Keycloak authentication

Authentication is disabled by default:

~~~json
{
  "Authentication": {
    "Enabled": false
  }
}
~~~

Enable it with:

~~~json
{
  "Authentication": {
    "Enabled": true,
    "Authority": "http://localhost:8080/realms/company-template",
    "Audience": "company-template-api",
    "RequireHttpsMetadata": false,
    "RoleClaimType": "roles"
  }
}
~~~

The API validates bearer tokens. It does not perform browser login and does not use cookie authentication.

Example authorization policies:

- `products.read`
- `products.write`

## OpenAPI

In development:

~~~text
/openapi/v1.json
~~~

The OpenAPI document includes bearer-token metadata for secured endpoint testing.

## Migrations

Add migrations from the Infrastructure project with the API as startup project:

~~~bash
dotnet ef migrations add InitialCreate --project src/Company.Template.Infrastructure --startup-project src/Company.Template.Api --output-dir Persistence/Migrations
~~~

Apply migrations:

~~~bash
dotnet ef database update --project src/Company.Template.Infrastructure --startup-project src/Company.Template.Api
~~~

For local Aspire runs, keep migration execution explicit unless your team intentionally adds development-only automatic migration execution.

## Tests

Run:

~~~bash
dotnet test
~~~

Integration tests use Testcontainers and PostgreSQL.

Do not use EF Core InMemory as a substitute for relational integration tests.

## Central package management

Package versions are centralized in:

~~~text
Directory.Packages.props
~~~

Project files reference packages without versions.

## Adding a new feature

1. Put business invariants and behavior in `Domain`.
2. Add use cases in `Application`.
3. Add EF Core mapping and persistence implementation in `Infrastructure` only when needed.
4. Add API request/response DTOs and endpoints under `Api/Endpoints/{Feature}`.
5. Add tests at the appropriate layer.

Keep endpoint handlers thin. Do not expose domain entities or EF entities directly from the API.
'@

Write-Status "Writing dotnet template configuration..."

Write-File ".template_config/template.json" @'
{
  "$schema": "http://json.schemastore.org/template",
  "author": "Codefox",
  "classifications": [ "Web", "WebAPI", "Clean Architecture", "DDD", "Aspire" ],
  "identity": "Codefox.CleanApi.Template",
  "name": "Clean Architecture Web API",
  "shortName": "cleanapi",
  "sourceName": "Company.Template",
  "preferNameDirectory": true,
  "description": "Production-oriented Clean Architecture Web API template with EF Core, Aspire, Keycloak JWT bearer wiring, Testcontainers and sample DDD code.",
  "tags": {
    "language": "C#",
    "type": "project"
  },
  "symbols": {
    "db": {
      "type": "parameter",
      "datatype": "choice",
      "description": "Database provider used by the generated application.",
      "defaultValue": "PostgreSql",
      "replaces": "__DB_PROVIDER__",
      "choices": [
        { "choice": "PostgreSql", "description": "Use PostgreSQL with Npgsql." },
        { "choice": "SqlServer", "description": "Use SQL Server." },
        { "choice": "MySql", "description": "Use MySQL/MariaDB with Pomelo." }
      ]
    }
  },
  "sources": [
    {
      "source": "./",
      "target": "./",
      "exclude": [
        "**/[Bb]in/**",
        "**/[Oo]bj/**",
        ".template_config/**/*"
      ]
    }
  ]
}
'@

} finally {
    Pop-Location
}

function Write-RootFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content
    )

    Write-File (Join-Path $TemplateRoot $Path) $Content
}

Write-Status "Writing template package project..."

Write-RootFile "template-package/Codefox.CleanApi.Template.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <PackageType>Template</PackageType>
    <PackageId>Codefox.CleanApi.Template</PackageId>
    <Title>Clean Architecture Web API Template</Title>
    <Authors>Codefox</Authors>
    <Description>Production-oriented Clean Architecture Web API dotnet new template.</Description>
    <PackageTags>dotnet-new;template;webapi;clean-architecture;aspire;ef-core;ddd</PackageTags>
    <PackageVersion>0.1.0</PackageVersion>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <IncludeContentInPack>true</IncludeContentInPack>
    <NoDefaultExcludes>true</NoDefaultExcludes>
  </PropertyGroup>

  <ItemGroup>
    <Content Include="..\content\**\*" Exclude="..\content\**\bin\**;..\content\**\obj\**" PackagePath="content\" />
  </ItemGroup>
</Project>
'@

Write-Status "Writing template repository README..."

Write-RootFile "README.md" @'
# Codefox Clean API Template

This repository contains a `dotnet new` template for a Clean Architecture Web API.

The outer repository is only the template authoring container. The generated template content lives in `content/` and is packaged by `template-package/`.

## Build the template package

~~~bash
dotnet pack ./template-package/Codefox.CleanApi.Template.csproj -c Release
~~~

## Install locally from the generated package

~~~bash
dotnet new install ./template-package/bin/Release/Codefox.CleanApi.Template.0.1.0.nupkg
~~~

## Generate a new API

~~~bash
dotnet new cleanapi -n Acme.Products --db PostgreSql
~~~

Supported database provider values:

- `PostgreSql`
- `SqlServer`
- `MySql`

## Notes

The template includes all EF Core providers centrally. The generated application selects the active provider through configuration.

Aspire, tests, and Keycloak JWT bearer wiring are included in this first version. Keycloak is disabled by default and can be enabled through configuration. This avoids half-working template parameters that exclude folders but leave stale solution/project references behind. Once the base template is stable, optional pruning can be added deliberately.
'@

Write-Status "Packing template package..."
dotnet pack (Join-Path $PackageRoot "Codefox.CleanApi.Template.csproj") -c Release

Write-Host "Template repository creation completed successfully." -ForegroundColor Green
Write-Host "Install with:" -ForegroundColor Green
Write-Host "dotnet new install ./template-package/bin/Release/Codefox.CleanApi.Template.0.1.0.nupkg" -ForegroundColor Green


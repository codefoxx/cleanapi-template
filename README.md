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

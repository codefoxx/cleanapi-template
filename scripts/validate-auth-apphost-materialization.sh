#!/usr/bin/env bash
set -Eeuo pipefail

REPOSITORY_ROOT="${REPOSITORY_ROOT:-$(pwd)}"
CONTENT_ROOT="$REPOSITORY_ROOT/content"
OUTPUT_ROOT="${OUTPUT_ROOT:-/tmp/auth-apphost-validation}"
REPORT="${REPORT:-$OUTPUT_ROOT/auth-apphost-validation-report.txt}"

NONE_PROJECT="$OUTPUT_ROOT/none"
KEYCLOAK_PROJECT="$OUTPUT_ROOT/keycloak"

mkdir -p "$OUTPUT_ROOT"
rm -f "$REPORT"

exec > >(tee "$REPORT") 2>&1

print_header() {
  echo
  echo "============================================================"
  echo "$1"
  echo "============================================================"
}

run() {
  echo
  echo "$ $*"
  "$@"
}

assert_no_findings() {
  local label="$1"
  local pattern="$2"
  shift 2

  echo
  echo "-- $label --"

  local findings
  findings="$(grep -RInE "$pattern" "$@" 2>/dev/null || true)"

  if [[ -n "$findings" ]]; then
    echo "$findings"
    echo
    echo "FAILED: unexpected findings for $label"
    exit 1
  fi

  echo "OK: no findings"
}

assert_findings() {
  local label="$1"
  local pattern="$2"
  shift 2

  echo
  echo "-- $label --"

  local findings
  findings="$(grep -RInE "$pattern" "$@" 2>/dev/null || true)"

  if [[ -z "$findings" ]]; then
    echo "FAILED: expected findings for $label"
    exit 1
  fi

  echo "$findings"
  echo
  echo "OK: expected findings found"
}

assert_no_files() {
  local label="$1"
  shift

  echo
  echo "-- $label --"

  local findings
  findings="$("$@" 2>/dev/null || true)"

  if [[ -n "$findings" ]]; then
    echo "$findings"
    echo
    echo "FAILED: unexpected files for $label"
    exit 1
  fi

  echo "OK: no findings"
}

assert_files() {
  local label="$1"
  shift

  echo
  echo "-- $label --"

  local findings
  findings="$("$@" 2>/dev/null || true)"

  if [[ -z "$findings" ]]; then
    echo "FAILED: expected files for $label"
    exit 1
  fi

  echo "$findings"
  echo
  echo "OK: expected files found"
}

assert_file_exists() {
  local label="$1"
  local file="$2"

  echo
  echo "-- $label --"

  if [[ ! -f "$file" ]]; then
    echo "FAILED: expected file does not exist: $file"
    exit 1
  fi

  echo "OK: file exists"
}

assert_file_absent() {
  local label="$1"
  local file="$2"

  echo
  echo "-- $label --"

  if [[ -f "$file" ]]; then
    echo "FAILED: file should not exist: $file"
    exit 1
  fi

  echo "OK: file does not exist"
}

print_file_if_exists() {
  local label="$1"
  local file="$2"

  echo
  echo "-- $label --"
  echo "File: $file"

  if [[ ! -f "$file" ]]; then
    echo "OK: file does not exist"
    return
  fi

  echo '```'
  sed -n '1,260p' "$file"
  echo '```'
}

print_header "Auth AppHost materialization validation"

echo "Repository root: $REPOSITORY_ROOT"
echo "Content root:    $CONTENT_ROOT"
echo "Output root:     $OUTPUT_ROOT"
echo "Report:          $REPORT"

cd "$REPOSITORY_ROOT"

echo
echo "Current branch:"
git branch --show-current

echo
echo "Current HEAD:"
git --no-pager log -1 --oneline

print_header "Raw content build"
run dotnet build "$CONTENT_ROOT"

print_header "Materialize projects"
run dotnet new install "$CONTENT_ROOT" --force

rm -rf "$NONE_PROJECT" "$KEYCLOAK_PROJECT"

run dotnet new cleanapi \
  -n Auth.AppHost.None \
  --auth None \
  -o "$NONE_PROJECT" \
  --force

run dotnet new cleanapi \
  -n Auth.AppHost.Keycloak \
  --auth Keycloak \
  -o "$KEYCLOAK_PROJECT" \
  --force

print_header "Check --auth None"

NONE_KEYCLOAK_PATTERN='Keycloak|keycloak|StartKeycloak|__KEYCLOAK|__AUTH_AUDIENCE__|Authentication__'

assert_no_findings \
  "generated --auth None AppHost Keycloak leftovers" \
  "$NONE_KEYCLOAK_PATTERN" \
  "$NONE_PROJECT/src/Auth.AppHost.None.AppHost"

if [[ -d "$NONE_PROJECT/infra" ]]; then
  assert_no_findings \
    "generated --auth None infra Keycloak leftovers" \
    "$NONE_KEYCLOAK_PATTERN" \
    "$NONE_PROJECT/infra"
else
  echo
  echo "-- generated --auth None infra directory --"
  echo "OK: infra directory does not exist"
fi

assert_no_files \
  "generated --auth None source variant leftovers" \
  find "$NONE_PROJECT" \
    \( -name '*.None.cs' -o -name '*.PostgreSql.cs' -o -name '*.SqlServer.cs' \) -print

assert_file_absent \
  "generated --auth None ApiAdapterServiceModule.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/ApiAdapterServiceModule.cs"

assert_file_absent \
  "generated --auth None ICurrentUser.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Application/Abstractions/Security/ICurrentUser.cs"

assert_file_absent \
  "generated --auth None HttpCurrentUser.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/CurrentUser/HttpCurrentUser.cs"

assert_file_absent \
  "generated --auth None AuthenticationOptions.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/Options/AuthenticationOptions.cs"

assert_file_absent \
  "generated --auth None AuthenticationExtensions.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/Security/AuthenticationExtensions.cs"

assert_file_absent \
  "generated --auth None TemplatePolicies.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/Security/TemplatePolicies.cs"

assert_file_absent \
  "generated --auth None OAuth2SecuritySchemeTransformer.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs"

print_header "Check --auth Keycloak"

assert_files \
  "generated --auth Keycloak Keycloak files" \
  find "$KEYCLOAK_PROJECT" \
    \( -path '*keycloak*' -o -name '*Keycloak*' \) -print

assert_findings \
  "generated --auth Keycloak AppHost wiring" \
  'AddTemplateKeycloak|WithTemplateKeycloakAuthentication|Authentication__Authority|Authentication__Audience|auth-apphost-keycloak' \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.AppHost" \
  "$KEYCLOAK_PROJECT/infra"

assert_no_findings \
  "generated --auth Keycloak old runtime switch leftovers" \
  'StartKeycloak|Authentication__Enabled' \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.AppHost" \
  "$KEYCLOAK_PROJECT/infra"

assert_no_files \
  "generated --auth Keycloak source variant leftovers" \
  find "$KEYCLOAK_PROJECT" \
    \( -name '*.None.cs' -o -name '*.PostgreSql.cs' -o -name '*.SqlServer.cs' \) -print

assert_file_exists \
  "generated --auth Keycloak ApiAdapterServiceModule.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/ApiAdapterServiceModule.cs"

assert_file_exists \
  "generated --auth Keycloak ICurrentUser.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Application/Abstractions/Security/ICurrentUser.cs"

assert_file_exists \
  "generated --auth Keycloak HttpCurrentUser.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/CurrentUser/HttpCurrentUser.cs"

assert_file_exists \
  "generated --auth Keycloak AuthenticationOptions.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/Options/AuthenticationOptions.cs"

assert_file_exists \
  "generated --auth Keycloak AuthenticationExtensions.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/Security/AuthenticationExtensions.cs"

assert_file_exists \
  "generated --auth Keycloak TemplatePolicies.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/Security/TemplatePolicies.cs"

assert_file_exists \
  "generated --auth Keycloak OAuth2SecuritySchemeTransformer.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs"

print_header "Inspect generated --auth None files"

print_file_if_exists \
  "--auth None AppHost Program.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.AppHost/Program.cs"

print_file_if_exists \
  "--auth None AppHost AppHostNames.cs" \
  "$NONE_PROJECT/src/Auth.AppHost.None.AppHost/AppHostNames.cs"

print_file_if_exists \
  "--auth None AppHost csproj" \
  "$NONE_PROJECT/src/Auth.AppHost.None.AppHost/Auth.AppHost.None.AppHost.csproj"

print_file_if_exists \
  "--auth None OpenAPI registration" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/OpenApi/OpenApiExtensions.cs"

print_file_if_exists \
  "--auth None Product endpoints" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/Endpoints/Products/ProductEndpoints.cs"

print_file_if_exists \
  "--auth None ApiAdapterServiceModule.cs should not exist" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/ApiAdapterServiceModule.cs"

print_file_if_exists \
  "--auth None ICurrentUser.cs should not exist" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Application/Abstractions/Security/ICurrentUser.cs"

print_file_if_exists \
  "--auth None HttpCurrentUser.cs should not exist" \
  "$NONE_PROJECT/src/Auth.AppHost.None.Api/CurrentUser/HttpCurrentUser.cs"

print_header "Inspect generated --auth Keycloak files"

print_file_if_exists \
  "--auth Keycloak AppHost Program.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.AppHost/Program.cs"

print_file_if_exists \
  "--auth Keycloak AppHost AppHostNames.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.AppHost/AppHostNames.cs"

print_file_if_exists \
  "--auth Keycloak AppHost csproj" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.AppHost/Auth.AppHost.Keycloak.AppHost.csproj"

print_file_if_exists \
  "--auth Keycloak OpenAPI registration" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/OpenApi/OpenApiExtensions.cs"

print_file_if_exists \
  "--auth Keycloak Product endpoints" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/Endpoints/Products/ProductEndpoints.cs"

print_file_if_exists \
  "--auth Keycloak ApiAdapterServiceModule.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/ApiAdapterServiceModule.cs"

print_file_if_exists \
  "--auth Keycloak ICurrentUser.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Application/Abstractions/Security/ICurrentUser.cs"

print_file_if_exists \
  "--auth Keycloak HttpCurrentUser.cs" \
  "$KEYCLOAK_PROJECT/src/Auth.AppHost.Keycloak.Api/CurrentUser/HttpCurrentUser.cs"

print_header "Build materialized projects"
run dotnet build "$NONE_PROJECT"
run dotnet build "$KEYCLOAK_PROJECT"

print_header "Result"
echo "Overall result: OK"
echo "Report written to: $REPORT"
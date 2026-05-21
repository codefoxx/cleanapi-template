#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

TEMPLATE_PROJECT="$REPO_ROOT/template-package/Codefox.CleanApi.Template.csproj"
TOOL_MANIFEST="$REPO_ROOT/.config/dotnet-tools.json"

TEST_ROOT="${TEST_ROOT:-/tmp/cleanapi-template}"
PROJECT_NAME="${PROJECT_NAME:-Acme.Products}"
DB_PROVIDER="${DB_PROVIDER:-PostgreSql}"
AUTH_PROVIDER="${AUTH_PROVIDER:-Keycloak}"
CONFIGURATION="${CONFIGURATION:-Release}"

DOTNET_EF_MODE=""

print_usage() {
  cat <<EOF
Usage:
  ./scripts/template.sh <command> [options]

Commands:
  pack       Build the template NuGet package.
  install    Pack and install the template locally.
  create     Create a generated validation project from the installed template.
  migrate    Create an EF Core migration in the generated validation project.
  build      Install, create, migrate and build the generated project.
  test       Build the generated project and run all tests.
  all        Run local validation close to the CI/materialization workflows.
  clean      Remove generated validation output.

Options:
  --db <provider>       Database provider: PostgreSql, SqlServer
  --auth <provider>     Authentication provider: Keycloak, None
  --name <name>         Generated project name
  --test-root <path>    Generated validation output root
  -c|--configuration    Build configuration
  -h|--help             Show help

Environment variables:
  DB_PROVIDER           Default: PostgreSql
  AUTH_PROVIDER         Default: Keycloak
  PROJECT_NAME          Default: Acme.Products
  TEST_ROOT             Default: /tmp/cleanapi-template
  CONFIGURATION         Default: Release

Examples:
  ./scripts/template.sh --help
  ./scripts/template.sh pack
  ./scripts/template.sh install
  ./scripts/template.sh create --db PostgreSql --auth Keycloak
  ./scripts/template.sh build --db PostgreSql
  ./scripts/template.sh test --db PostgreSql
  ./scripts/template.sh test --db SqlServer --name Acme.SqlServer
  ./scripts/template.sh build --db PostgreSql --auth None --name Acme.NoAuth
  ./scripts/template.sh all
EOF
}

parse_options() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --db)
        require_option_value "$1" "${2:-}"
        DB_PROVIDER="$2"
        shift 2
        ;;
      --auth)
        require_option_value "$1" "${2:-}"
        AUTH_PROVIDER="$2"
        shift 2
        ;;
      --name)
        require_option_value "$1" "${2:-}"
        PROJECT_NAME="$2"
        shift 2
        ;;
      --test-root)
        require_option_value "$1" "${2:-}"
        TEST_ROOT="$2"
        shift 2
        ;;
      -c|--configuration)
        require_option_value "$1" "${2:-}"
        CONFIGURATION="$2"
        shift 2
        ;;
      -h|--help)
        print_usage
        exit 0
        ;;
      *)
        echo "ERROR: Unknown option: $1"
        print_usage
        exit 1
        ;;
    esac
  done
}

require_option_value() {
  local option="$1"
  local value="$2"

  if [[ -z "$value" || "$value" == --* ]]; then
    echo "ERROR: Missing value for option: $option"
    print_usage
    exit 1
  fi
}

validate_db_provider() {
  case "$DB_PROVIDER" in
    PostgreSql|SqlServer)
      ;;
    *)
      echo "ERROR: Unsupported DB provider: $DB_PROVIDER"
      echo "Supported providers: PostgreSql, SqlServer"
      exit 1
      ;;
  esac
}

validate_auth_provider() {
  case "$AUTH_PROVIDER" in
    Keycloak|None)
      ;;
    *)
      echo "ERROR: Unsupported auth provider: $AUTH_PROVIDER"
      echo "Supported providers: Keycloak, None"
      exit 1
      ;;
  esac
}

generated_project_dir() {
  echo "$TEST_ROOT/$PROJECT_NAME"
}

generated_solution_path() {
  echo "$(generated_project_dir)/$PROJECT_NAME.slnx"
}

pack_template() {
  echo "==> Packing template"

  dotnet msbuild "$TEMPLATE_PROJECT" \
    /target:Pack \
    /property:Configuration="$CONFIGURATION" \
    /terminalLogger:off \
    /verbosity:minimal
}

install_template() {
  echo "==> Installing template"

  dotnet msbuild "$TEMPLATE_PROJECT" \
    /target:InstallTemplate \
    /property:Configuration="$CONFIGURATION" \
    /terminalLogger:off \
    /verbosity:minimal
}

restore_dotnet_tools() {
  if [[ -f "$TOOL_MANIFEST" ]]; then
    echo "==> Restoring local .NET tools"

    dotnet tool restore --tool-manifest "$TOOL_MANIFEST"
    DOTNET_EF_MODE="local"
    return
  fi

  if dotnet ef --version >/dev/null 2>&1; then
    DOTNET_EF_MODE="global"
    return
  fi

  echo "ERROR: dotnet-ef is not available."
  echo
  echo "Install it globally:"
  echo "  dotnet tool install --global dotnet-ef"
  echo
  echo "Or add a local tool manifest to the repository:"
  echo "  dotnet new tool-manifest"
  echo "  dotnet tool install dotnet-ef"
  exit 1
}

run_dotnet_ef() {
  if [[ -z "$DOTNET_EF_MODE" ]]; then
    restore_dotnet_tools
  fi

  case "$DOTNET_EF_MODE" in
    local)
      (
        cd "$REPO_ROOT"
        dotnet tool run dotnet-ef -- "$@"
      )
      ;;
    global)
      dotnet ef "$@"
      ;;
    *)
      echo "ERROR: Unknown dotnet-ef mode: $DOTNET_EF_MODE"
      exit 1
      ;;
  esac
}

build_template_content() {
  echo "==> Restoring raw template content"

  dotnet restore "$REPO_ROOT/content/Company.Template.slnx" \
    /p:EffectiveDbProvider=PostgreSql

  echo "==> Building raw template content"

  dotnet build "$REPO_ROOT/content/Company.Template.slnx" \
    --configuration "$CONFIGURATION" \
    --no-restore \
    /p:EffectiveDbProvider=PostgreSql
}

test_template_content() {
  echo "==> Testing raw template content"

  dotnet test "$REPO_ROOT/content/Company.Template.slnx" \
    --configuration "$CONFIGURATION" \
    --no-build \
    /p:EffectiveDbProvider=PostgreSql
}

clean_test_output() {
  echo "==> Cleaning generated validation output: $TEST_ROOT"

  rm -rf "$TEST_ROOT"
  mkdir -p "$TEST_ROOT"
}

clean_generated_project_output() {
  local project_dir
  project_dir="$(generated_project_dir)"

  echo "==> Cleaning generated project output: $project_dir"

  rm -rf "$project_dir"
  mkdir -p "$TEST_ROOT"
}

create_test_project() {
  validate_db_provider
  validate_auth_provider
  clean_generated_project_output

  echo "==> Creating project: $PROJECT_NAME with db provider: $DB_PROVIDER and auth provider: $AUTH_PROVIDER"

  dotnet new cleanapi \
    -n "$PROJECT_NAME" \
    --db "$DB_PROVIDER" \
    --auth "$AUTH_PROVIDER" \
    -o "$(generated_project_dir)" \
    --force

  cd "$(generated_project_dir)"

  check_unresolved_placeholders
  list_provider_specific_files
  verify_migration_service_exists
}

check_unresolved_placeholders() {
  echo "==> Checking for unresolved template placeholders"

  if grep -R "Company.Template\|__DB_PROVIDER__\|__API_RESOURCE_NAME__\|__DATABASE_RESOURCE_NAME__\|__MIGRATION_SERVICE_RESOURCE_NAME__\|__PGADMIN_RESOURCE_NAME__\|__KEYCLOAK_RESOURCE_NAME__\|__KEYCLOAK_REALM__\|__AUTH_AUDIENCE__" . \
    --exclude-dir=bin \
    --exclude-dir=obj \
    --exclude-dir=.git; then
    echo "ERROR: Unresolved template placeholders found."
    exit 1
  fi
}

list_provider_specific_files() {
  echo "==> Listing provider-specific files"

  find . -name "*DatabaseProviderConfigurator*.cs" -print
  find . -name "TestDatabase*.cs" -print
}

verify_migration_service_exists() {
  echo "==> Verifying migration service project exists"

  test -f "src/aspire/$PROJECT_NAME.MigrationService/$PROJECT_NAME.MigrationService.csproj"
}

restore_test_project() {
  echo "==> Restore generated project"

  dotnet restore "$(generated_solution_path)"
}

configure_design_time_database() {
  echo "==> Setting design-time database configuration for migration generation"

  export Database__Provider="$DB_PROVIDER"

  case "$DB_PROVIDER" in
    PostgreSql)
      export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=dummy;Username=dummy;Password=dummy"
      ;;
    SqlServer)
      export ConnectionStrings__DefaultConnection="Server=localhost;Database=dummy;User Id=sa;Password=Dummy_password123;TrustServerCertificate=True"
      ;;
  esac
}

create_initial_migration() {
  validate_db_provider

  restore_test_project
  configure_design_time_database

  local infrastructure_project
  local migration_service_project

  infrastructure_project="$(generated_project_dir)/src/$PROJECT_NAME.Infrastructure/$PROJECT_NAME.Infrastructure.csproj"
  migration_service_project="$(generated_project_dir)/src/aspire/$PROJECT_NAME.MigrationService/$PROJECT_NAME.MigrationService.csproj"

  echo "==> Creating initial EF Core migration"

  run_dotnet_ef migrations add InitialCreate \
    --project "$infrastructure_project" \
    --startup-project "$migration_service_project" \
    --context ApplicationDbContext \
    --output-dir Persistence/Migrations

  clean_design_time_artifacts
  verify_migration_files_created
  verify_no_build_host_artifacts
}

clean_design_time_artifacts() {
  echo "==> Cleaning EF Core design-time build artifacts"

  find "$(generated_project_dir)" -type d -name "BuildHost-*" -prune -exec rm -rf {} +
}

verify_migration_files_created() {
  echo "==> Verifying migration files were created"

  local migrations_dir
  migrations_dir="$(generated_project_dir)/src/$PROJECT_NAME.Infrastructure/Persistence/Migrations"

  if ! find "$migrations_dir" -name "*InitialCreate*.cs" | grep -q .; then
    echo "ERROR: InitialCreate migration was not created."
    exit 1
  fi

  if ! find "$migrations_dir" -name "*ModelSnapshot.cs" | grep -q .; then
    echo "ERROR: EF Core model snapshot was not created."
    exit 1
  fi
}

verify_no_build_host_artifacts() {
  echo "==> Checking for build host artifacts"

  if find "$(generated_project_dir)" -type d -name "BuildHost-*" -print | grep -q .; then
    echo "ERROR: BuildHost artifacts found."
    exit 1
  fi
}

build_test_project() {
  echo "==> Build generated project"

  dotnet build "$(generated_solution_path)" --no-restore
}

run_generated_tests() {
  echo "==> Test generated project"

  dotnet test "$(generated_solution_path)" --no-build
}

run_template_build() {
  install_template
  restore_dotnet_tools
  create_test_project
  create_initial_migration
  build_test_project

  echo "==> Template build succeeded"
}

run_template_test() {
  run_template_build
  run_generated_tests

  echo "==> Template test succeeded"
}

run_generated_project_build_without_install() {
  create_test_project
  create_initial_migration
  build_test_project

  echo "==> Generated project build succeeded"
}

run_generated_project_test_without_install() {
  run_generated_project_build_without_install
  run_generated_tests

  echo "==> Generated project test succeeded"
}

with_generated_context() {
  local db_provider="$1"
  local auth_provider="$2"
  local project_name="$3"
  shift 3

  local previous_db_provider="$DB_PROVIDER"
  local previous_auth_provider="$AUTH_PROVIDER"
  local previous_project_name="$PROJECT_NAME"

  DB_PROVIDER="$db_provider"
  AUTH_PROVIDER="$auth_provider"
  PROJECT_NAME="$project_name"

  "$@"

  DB_PROVIDER="$previous_db_provider"
  AUTH_PROVIDER="$previous_auth_provider"
  PROJECT_NAME="$previous_project_name"
}

run_provider_template_test() {
  local db_provider="$1"
  local project_name="$2"

  echo "==> Running full generated project validation for $project_name ($db_provider, Keycloak)"

  with_generated_context "$db_provider" "Keycloak" "$project_name" run_generated_project_test_without_install
}

materialization_dir() {
  local kind="$1"

  echo "$TEST_ROOT/materialization/$kind"
}

auth_materialization_dir() {
  local kind="$1"

  echo "$TEST_ROOT/auth-materialization/$kind"
}

materialize_project() {
  local project_name="$1"
  local db_provider="$2"
  local auth_provider="$3"
  local output_dir="$4"

  echo "==> Materializing $project_name ($db_provider, $auth_provider) into $output_dir"

  rm -rf "$output_dir"
  mkdir -p "$(dirname "$output_dir")"

  dotnet new cleanapi \
    -n "$project_name" \
    --db "$db_provider" \
    --auth "$auth_provider" \
    -o "$output_dir" \
    --force
}

validate_provider_materialization() {
  echo "==> Validating provider materialization"

  local postgresql_project
  local sqlserver_project

  postgresql_project="$(materialization_dir postgresql)"
  sqlserver_project="$(materialization_dir sqlserver)"

  materialize_project "Acme.PostgreSql" "PostgreSql" "Keycloak" "$postgresql_project"
  materialize_project "Acme.SqlServer" "SqlServer" "Keycloak" "$sqlserver_project"

  assert_no_provider_suffixes "$postgresql_project" "PostgreSQL output"
  assert_no_provider_suffixes "$sqlserver_project" "SQL Server output"

  assert_provider_neutral_files_exist "$postgresql_project" "Acme.PostgreSql"
  assert_provider_neutral_files_exist "$sqlserver_project" "Acme.SqlServer"

  assert_no_authoring_conditions "$postgresql_project" "PostgreSQL output"
  assert_no_authoring_conditions "$sqlserver_project" "SQL Server output"

  assert_no_wrong_provider_packages \
    "$postgresql_project" \
    "PostgreSQL output" \
    'Aspire\.Hosting\.SqlServer|Microsoft\.Data\.SqlClient|Microsoft\.EntityFrameworkCore\.SqlServer|Testcontainers\.MsSql'

  assert_no_wrong_provider_packages \
    "$sqlserver_project" \
    "SQL Server output" \
    'Aspire\.Hosting\.PostgreSQL|Npgsql|Testcontainers\.PostgreSql'

  echo "==> Provider materialization validation succeeded"
}

assert_no_provider_suffixes() {
  local project="$1"
  local label="$2"

  echo "==> Checking provider-specific filenames are absent: $label"

  local findings
  findings="$({
    find "$project" -type f \( \
      -name "*.PostgreSql.cs" \
      -o -name "*.SqlServer.cs" \
      -o -name "PostgreSqlAspireDatabase.cs" \
      -o -name "SqlServerAspireDatabase.cs" \
    \) -print
  })"

  if [[ -n "$findings" ]]; then
    echo "$findings"
    echo "ERROR: Provider-specific filename leftovers found in $label."
    exit 1
  fi
}

assert_provider_neutral_files_exist() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking neutral provider files exist: $project_name"

  assert_file_exists "$project/src/aspire/$project_name.AppHost/Providers/AspireDatabase.cs"
  assert_file_exists "$project/src/$project_name.Infrastructure/Persistence/Providers/DatabaseProviderConfigurator.cs"
  assert_file_exists "$project/tests/$project_name.TestSupport/Database/TestDatabaseProvider.cs"
  assert_file_exists "$project/tests/$project_name.TestSupport/Database/TestDatabaseServer.cs"
}

assert_no_authoring_conditions() {
  local project="$1"
  local label="$2"

  echo "==> Checking authoring conditions are absent: $label"

  if grep -RInE 'EffectiveDbProvider|Condition=.*PostgreSql|Condition=.*SqlServer' \
    "$project" \
    --include='*.csproj' \
    --include='*.props'; then
    echo "ERROR: Authoring provider conditions found in $label."
    exit 1
  fi
}

assert_no_wrong_provider_packages() {
  local project="$1"
  local label="$2"
  local pattern="$3"

  echo "==> Checking wrong-provider packages are absent: $label"

  if grep -RInE "$pattern" \
    "$project" \
    --include='*.csproj' \
    --include='*.props'; then
    echo "ERROR: Wrong-provider packages found in $label."
    exit 1
  fi
}

validate_auth_materialization() {
  echo "==> Validating auth materialization"

  local none_project
  local keycloak_project

  none_project="$(auth_materialization_dir none)"
  keycloak_project="$(auth_materialization_dir keycloak)"

  materialize_project "Acme.NoAuth" "PostgreSql" "None" "$none_project"
  materialize_project "Acme.Keycloak" "PostgreSql" "Keycloak" "$keycloak_project"

  assert_no_auth_excludes_auth_artifacts "$none_project" "Acme.NoAuth"
  assert_no_auth_has_no_keycloak_leftovers "$none_project" "Acme.NoAuth"
  assert_keycloak_includes_auth_artifacts "$keycloak_project" "Acme.Keycloak"
  assert_keycloak_apphost_wiring "$keycloak_project" "Acme.Keycloak"
  assert_old_auth_runtime_switches_absent "$keycloak_project" "Acme.Keycloak"
  assert_source_variant_leftovers_absent "$none_project" "$keycloak_project"

  echo "==> Building no-auth generated project"
  dotnet build "$none_project/Acme.NoAuth.slnx"

  echo "==> Building Keycloak generated project"
  dotnet build "$keycloak_project/Acme.Keycloak.slnx"

  echo "==> Auth materialization validation succeeded"
}

assert_no_auth_excludes_auth_artifacts() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking no-auth excludes auth artifacts"

  test ! -d "$project/infra"
  test ! -f "$project/src/$project_name.Api/ApiAdapterServiceModule.cs"
  test ! -f "$project/src/$project_name.Application/Abstractions/Security/ICurrentUser.cs"
  test ! -f "$project/src/$project_name.Api/CurrentUser/HttpCurrentUser.cs"
  test ! -f "$project/src/$project_name.Api/Options/AuthenticationOptions.cs"
  test ! -f "$project/src/$project_name.Api/Security/AuthenticationExtensions.cs"
  test ! -f "$project/src/$project_name.Api/Security/TemplatePolicies.cs"
  test ! -f "$project/src/$project_name.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs"
}

assert_no_auth_has_no_keycloak_leftovers() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking no-auth has no Keycloak/Auth leftovers"

  if grep -RInE 'Keycloak|keycloak|StartKeycloak|__KEYCLOAK|__AUTH_AUDIENCE__|Authentication__' \
    "$project/src/aspire/$project_name.AppHost"; then
    echo "ERROR: no-auth AppHost contains Keycloak/Auth leftovers."
    exit 1
  fi
}

assert_keycloak_includes_auth_artifacts() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking Keycloak includes auth artifacts"

  assert_file_exists "$project/infra/keycloak/realms/acme-keycloak-realm.json"
  assert_file_exists "$project/src/$project_name.Api/ApiAdapterServiceModule.cs"
  assert_file_exists "$project/src/$project_name.Application/Abstractions/Security/ICurrentUser.cs"
  assert_file_exists "$project/src/$project_name.Api/CurrentUser/HttpCurrentUser.cs"
  assert_file_exists "$project/src/$project_name.Api/Options/AuthenticationOptions.cs"
  assert_file_exists "$project/src/$project_name.Api/Security/AuthenticationExtensions.cs"
  assert_file_exists "$project/src/$project_name.Api/Security/TemplatePolicies.cs"
  assert_file_exists "$project/src/$project_name.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs"
}

assert_keycloak_apphost_wiring() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking Keycloak AppHost wiring"

  grep -RInE 'AddTemplateKeycloak|WithTemplateKeycloakAuthentication|Authentication__Authority|Authentication__Audience|acme-keycloak' \
    "$project/src/aspire/$project_name.AppHost" \
    "$project/infra"
}

assert_old_auth_runtime_switches_absent() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking old auth runtime switches are absent"

  if grep -RInE 'StartKeycloak|Authentication__Enabled' \
    "$project/src/aspire/$project_name.AppHost" \
    "$project/infra"; then
    echo "ERROR: old auth runtime switch leftovers found."
    exit 1
  fi
}

assert_source_variant_leftovers_absent() {
  local none_project="$1"
  local keycloak_project="$2"

  echo "==> Checking source variant leftovers are absent"

  if find "$none_project" "$keycloak_project" \
    \( -name '*.None.cs' -o -name '*.PostgreSql.cs' -o -name '*.SqlServer.cs' \) \
    -print | grep -q .; then
    echo "ERROR: source variant leftovers found."
    exit 1
  fi
}

assert_file_exists() {
  local file="$1"

  if [[ ! -f "$file" ]]; then
    echo "ERROR: Expected file does not exist: $file"
    exit 1
  fi
}

run_all() {
  clean_test_output

  build_template_content
  test_template_content

  pack_template
  install_template
  restore_dotnet_tools

  run_provider_template_test "PostgreSql" "Acme.PostgreSql"
  run_provider_template_test "SqlServer" "Acme.SqlServer"

  validate_provider_materialization
  validate_auth_materialization

  echo "==> Full template validation succeeded"
}

if [[ $# -eq 0 ]]; then
  print_usage
  exit 0
fi

COMMAND=""

case "$1" in
  pack|install|create|migrate|build|test|all|clean)
    COMMAND="$1"
    shift
    ;;
  -h|--help)
    print_usage
    exit 0
    ;;
  --*)
    echo "ERROR: Missing command."
    print_usage
    exit 1
    ;;
  *)
    echo "ERROR: Unknown command: $1"
    print_usage
    exit 1
    ;;
esac

parse_options "$@"

cd "$REPO_ROOT"

case "$COMMAND" in
  pack)
    pack_template
    ;;
  install)
    install_template
    ;;
  create)
    create_test_project
    ;;
  migrate)
    restore_dotnet_tools
    create_initial_migration
    ;;
  build)
    run_template_build
    ;;
  test)
    run_template_test
    ;;
  all)
    run_all
    ;;
  clean)
    clean_test_output
    ;;
esac
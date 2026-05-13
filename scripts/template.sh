#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

TEMPLATE_PROJECT="$REPO_ROOT/template-package/Codefox.CleanApi.Template.csproj"
TEST_ROOT="${TEST_ROOT:-/tmp/cleanapi-template-test}"
PROJECT_NAME="${PROJECT_NAME:-Acme.Products}"
DB_PROVIDER="${DB_PROVIDER:-PostgreSql}"
CONFIGURATION="${CONFIGURATION:-Release}"

print_usage() {
  cat <<EOF
Usage:
  ./scripts/template.sh <command> [options]

Commands:
  pack       Build the template NuGet package.
  install    Pack and install the template locally.
  create     Create a test project from the installed template.
  migrate    Create an EF Core migration in the test project.
  build      Install, create, migrate and build the generated project.
  test       Build the generated project and run all tests.
  all        Pack the template, build the generated project and run all tests.
  clean      Remove generated test output.

Options:
  --db <provider>       Database provider: PostgreSql, SqlServer
  --name <name>         Generated project name
  --test-root <path>    Test output folder
  -c|--configuration    Build configuration
  -h|--help             Show help

Environment variables:
  DB_PROVIDER           Default: PostgreSql
  PROJECT_NAME          Default: Acme.Products
  TEST_ROOT             Default: /tmp/cleanapi-template-test
  CONFIGURATION         Default: Release

Examples:
  ./scripts/template.sh --help
  ./scripts/template.sh pack
  ./scripts/template.sh install
  ./scripts/template.sh create --db PostgreSql
  ./scripts/template.sh build --db PostgreSql
  ./scripts/template.sh test --db PostgreSql
  ./scripts/template.sh all --db PostgreSql
  ./scripts/template.sh test --db SqlServer --name Acme.Orders
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

generated_project_dir() {
  echo "$TEST_ROOT/$PROJECT_NAME"
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

clean_test_output() {
  echo "==> Cleaning test output"

  rm -rf "$TEST_ROOT"
  mkdir -p "$TEST_ROOT"
}

create_test_project() {
  validate_db_provider
  clean_test_output

  echo "==> Creating project: $PROJECT_NAME with db provider: $DB_PROVIDER"

  cd "$TEST_ROOT"
  dotnet new cleanapi -n "$PROJECT_NAME" --db "$DB_PROVIDER"

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

  test -f "src/$PROJECT_NAME.MigrationService/$PROJECT_NAME.MigrationService.csproj"
}

restore_test_project() {
  echo "==> Restore"

  cd "$(generated_project_dir)"
  dotnet restore
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

  cd "$(generated_project_dir)"

  restore_test_project
  configure_design_time_database

  echo "==> Creating initial EF Core migration"

  dotnet ef migrations add InitialCreate \
    --project "src/$PROJECT_NAME.Infrastructure/$PROJECT_NAME.Infrastructure.csproj" \
    --startup-project "src/$PROJECT_NAME.MigrationService/$PROJECT_NAME.MigrationService.csproj" \
    --context ApplicationDbContext \
    --output-dir Persistence/Migrations

  clean_design_time_artifacts
  verify_migration_files_created
  verify_no_build_host_artifacts
}

clean_design_time_artifacts() {
  echo "==> Cleaning EF Core design-time build artifacts"

  find . -type d -name "BuildHost-*" -prune -exec rm -rf {} +
}

verify_migration_files_created() {
  echo "==> Verifying migration files were created"

  if ! find "src/$PROJECT_NAME.Infrastructure/Persistence/Migrations" -name "*InitialCreate*.cs" | grep -q .; then
    echo "ERROR: InitialCreate migration was not created."
    exit 1
  fi

  if ! find "src/$PROJECT_NAME.Infrastructure/Persistence/Migrations" -name "*ModelSnapshot.cs" | grep -q .; then
    echo "ERROR: EF Core model snapshot was not created."
    exit 1
  fi
}

verify_no_build_host_artifacts() {
  echo "==> Checking for build host artifacts"

  if find . -type d -name "BuildHost-*" -print | grep -q .; then
    echo "ERROR: BuildHost artifacts found."
    exit 1
  fi
}

build_test_project() {
  echo "==> Build"

  cd "$(generated_project_dir)"
  dotnet build --no-restore
}

run_generated_tests() {
  echo "==> Test"

  cd "$(generated_project_dir)"
  dotnet test --no-build
}

run_template_build() {
  install_template
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

run_all() {
  pack_template
  run_template_test

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
TEMPLATE_PROJECT="$REPO_ROOT/template-package/Codefox.CleanApi.Template.csproj"

TEST_ROOT="${TEST_ROOT:-/tmp/cleanapi-template}"
PROJECT_NAME="${PROJECT_NAME:-Acme.Products}"
DB_PROVIDER="${DB_PROVIDER:-PostgreSql}"
AUTH_PROVIDER="${AUTH_PROVIDER:-Keycloak}"
CONFIGURATION="${CONFIGURATION:-Release}"

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

materialization_dir() {
  local kind="$1"

  echo "$TEST_ROOT/materialization/$kind"
}

auth_materialization_dir() {
  local kind="$1"

  echo "$TEST_ROOT/auth-materialization/$kind"
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

assert_file_exists() {
  local file="$1"

  if [[ ! -f "$file" ]]; then
    echo "ERROR: Expected file does not exist: $file"
    exit 1
  fi
}

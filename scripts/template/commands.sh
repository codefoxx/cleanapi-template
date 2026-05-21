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

run_all_common_setup() {
  clean_test_output

  build_template_content
  test_template_content

  pack_template
  install_template
  restore_dotnet_tools
}

run_all() {
  run_all_common_setup

  run_provider_template_test "PostgreSql" "Acme.PostgreSql"
  run_provider_template_test "SqlServer" "Acme.SqlServer"

  validate_provider_materialization
  validate_auth_materialization

  echo "==> Full template validation succeeded"
}

run_all_postgres() {
  run_all_common_setup

  run_provider_template_test "PostgreSql" "Acme.PostgreSql"
  validate_auth_materialization

  echo "==> PostgreSQL template validation succeeded"
}

run_all_sqlserver() {
  run_all_common_setup

  run_provider_template_test "SqlServer" "Acme.SqlServer"
  validate_provider_materialization

  echo "==> SQL Server template validation succeeded"
}

main() {
  if [[ $# -eq 0 ]]; then
    print_usage
    exit 0
  fi

  local command=""

  case "$1" in
    pack|install|create|migrate|build|test|all|all-postgres|all-sqlserver|clean)
      command="$1"
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

  case "$command" in
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
    all-postgres)
      run_all_postgres
      ;;
    all-sqlserver)
      run_all_sqlserver
      ;;
    clean)
      clean_test_output
      ;;
  esac
}

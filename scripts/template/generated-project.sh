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

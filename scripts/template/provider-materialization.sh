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

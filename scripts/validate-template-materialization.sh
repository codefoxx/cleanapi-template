#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_ROOT="${TEMPLATE_VALIDATION_OUTPUT_ROOT:-/tmp/cleanapi-template-validation}"
PG_OUTPUT="$OUTPUT_ROOT/postgresql"
SQL_OUTPUT="$OUTPUT_ROOT/sqlserver"
REPORT="$OUTPUT_ROOT/template-materialization-report.txt"

mkdir -p "$OUTPUT_ROOT"
: > "$REPORT"

log() {
  echo "$*" | tee -a "$REPORT"
}

fail() {
  log "ERROR: $*"
  exit 1
}

run() {
  log ""
  log "$ $*"
  "$@" >> "$REPORT" 2>&1
}

assert_no_output() {
  local title="$1"
  shift

  log ""
  log "-- $title --"

  local output
  if output="$($@ 2>&1)" && [ -z "$output" ]; then
    log "OK: no findings"
    return 0
  fi

  if [ -z "$output" ]; then
    log "OK: no findings"
    return 0
  fi

  log "FINDINGS:"
  log "$output"
  fail "$title produced findings"
}

assert_file_exists() {
  local file="$1"

  if [ -f "$file" ]; then
    log "OK: $file"
    return 0
  fi

  fail "Expected file does not exist: $file"
}

validate_provider_suffixes_absent() {
  local project="$1"
  local label="$2"

  assert_no_output \
    "$label: provider-specific filename leftovers" \
    find "$project" -type f \( \
      -name "*.PostgreSql.cs" \
      -o -name "*.SqlServer.cs" \
      -o -name "PostgreSqlAspireDatabase.cs" \
      -o -name "SqlServerAspireDatabase.cs" \
    \) -print
}

validate_neutral_provider_files_exist() {
  local project="$1"
  local name="$2"
  local label="$3"

  log ""
  log "-- $label: neutral provider files --"

  assert_file_exists "$project/src/$name.AppHost/Providers/AspireDatabase.cs"
  assert_file_exists "$project/src/$name.Infrastructure/Persistence/Providers/DatabaseProviderConfigurator.cs"
  assert_file_exists "$project/tests/$name.TestSupport/Database/TestDatabaseProvider.cs"
  assert_file_exists "$project/tests/$name.TestSupport/Database/TestDatabaseServer.cs"
}

validate_authoring_conditions_absent() {
  local project="$1"
  local label="$2"

  assert_no_output \
    "$label: EffectiveDbProvider / provider MSBuild conditions" \
    grep -RInE 'EffectiveDbProvider|Condition=.*PostgreSql|Condition=.*SqlServer' \
      "$project" \
      --include='*.csproj' \
      --include='*.props'
}

validate_wrong_provider_packages_absent() {
  local project="$1"
  local label="$2"
  local forbidden_pattern="$3"

  assert_no_output \
    "$label: wrong-provider package references" \
    grep -RInE "$forbidden_pattern" \
      "$project" \
      --include='*.csproj' \
      --include='*.props'
}

log "============================================================"
log "Template materialization validation"
log "============================================================"
log "Repository root: $ROOT"
log "Output root: $OUTPUT_ROOT"
log "Report: $REPORT"

cd "$ROOT"

run dotnet new install ./content --force

rm -rf "$PG_OUTPUT" "$SQL_OUTPUT"

run dotnet new cleanapi -n Validation.PostgreSql --db PostgreSql -o "$PG_OUTPUT" --force
run dotnet new cleanapi -n Validation.SqlServer --db SqlServer -o "$SQL_OUTPUT" --force

validate_provider_suffixes_absent "$PG_OUTPUT" "PostgreSQL project"
validate_provider_suffixes_absent "$SQL_OUTPUT" "SQL Server project"

validate_neutral_provider_files_exist "$PG_OUTPUT" "Validation.PostgreSql" "PostgreSQL project"
validate_neutral_provider_files_exist "$SQL_OUTPUT" "Validation.SqlServer" "SQL Server project"

validate_authoring_conditions_absent "$PG_OUTPUT" "PostgreSQL project"
validate_authoring_conditions_absent "$SQL_OUTPUT" "SQL Server project"

validate_wrong_provider_packages_absent \
  "$PG_OUTPUT" \
  "PostgreSQL project" \
  'Aspire\.Hosting\.SqlServer|Microsoft\.Data\.SqlClient|Microsoft\.EntityFrameworkCore\.SqlServer|Testcontainers\.MsSql'

validate_wrong_provider_packages_absent \
  "$SQL_OUTPUT" \
  "SQL Server project" \
  'Aspire\.Hosting\.PostgreSQL|Npgsql|Testcontainers\.PostgreSql'

log ""
log "Overall result: OK"

#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TEST_ROOT="/tmp/cleanapi-template-test"
PROJECT_NAME="Acme.Products"
DB_PROVIDER="${1:-PostgreSql}"

cd "$REPO_ROOT"

echo "==> Installing template"
dotnet msbuild ./template-package/Codefox.CleanApi.Template.csproj \
  /target:InstallTemplate \
  /property:Configuration=Release \
  /terminalLogger:off \
  /verbosity:minimal

echo "==> Cleaning test output"
rm -rf "$TEST_ROOT"
mkdir -p "$TEST_ROOT"

cd "$TEST_ROOT"

echo "==> Creating project: $PROJECT_NAME with db provider: $DB_PROVIDER"
dotnet new cleanapi -n "$PROJECT_NAME" --db "$DB_PROVIDER"

cd "$PROJECT_NAME"

echo "==> Checking for unresolved template placeholders"
if grep -R "Company.Template\|__DB_PROVIDER__\|__API_RESOURCE_NAME__\|__DATABASE_RESOURCE_NAME__\|__MIGRATION_SERVICE_RESOURCE_NAME__\|__PGADMIN_RESOURCE_NAME__\|__KEYCLOAK_RESOURCE_NAME__\|__KEYCLOAK_REALM__\|__AUTH_AUDIENCE__" . \
  --exclude-dir=bin \
  --exclude-dir=obj \
  --exclude-dir=.git; then
  echo "ERROR: Unresolved template placeholders found."
  exit 1
fi

echo "==> Listing provider-specific files"
find . -name "*DatabaseProviderConfigurator*.cs" -print
find . -name "TestDatabase*.cs" -print

echo "==> Verifying migration service project exists"
test -f "src/$PROJECT_NAME.MigrationService/$PROJECT_NAME.MigrationService.csproj"

echo "==> Restore"
dotnet restore

echo "==> Setting design-time database configuration for migration generation"
export Database__Provider="$DB_PROVIDER"

case "$DB_PROVIDER" in
  PostgreSql)
    export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=dummy;Username=dummy;Password=dummy"
    ;;
  SqlServer)
    export ConnectionStrings__DefaultConnection="Server=localhost;Database=dummy;User Id=sa;Password=Dummy_password123;TrustServerCertificate=True"
    ;;
  MySql)
    export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=dummy;User=dummy;Password=dummy"
    ;;
  *)
    echo "ERROR: Unsupported DB provider: $DB_PROVIDER"
    exit 1
    ;;
esac

echo "==> Creating initial EF Core migration"
dotnet ef migrations add InitialCreate \
  --project "src/$PROJECT_NAME.Infrastructure/$PROJECT_NAME.Infrastructure.csproj" \
  --startup-project "src/$PROJECT_NAME.MigrationService/$PROJECT_NAME.MigrationService.csproj" \
  --context ApplicationDbContext \
  --output-dir Persistence/Migrations

echo "==> Cleaning EF Core design-time build artifacts"
find . -type d -name "BuildHost-*" -prune -exec rm -rf {} +

echo "==> Verifying migration files were created"
if ! find "src/$PROJECT_NAME.Infrastructure/Persistence/Migrations" -name "*InitialCreate*.cs" | grep -q .; then
  echo "ERROR: InitialCreate migration was not created."
  exit 1
fi

if ! find "src/$PROJECT_NAME.Infrastructure/Persistence/Migrations" -name "*ModelSnapshot.cs" | grep -q .; then
  echo "ERROR: EF Core model snapshot was not created."
  exit 1
fi

echo "==> Checking for build host artifacts"
if find . -type d -name "BuildHost-*" -print | grep -q .; then
  echo "ERROR: BuildHost artifacts found."
  exit 1
fi

echo "==> Build"
dotnet build --no-restore

echo "==> Template test succeeded"
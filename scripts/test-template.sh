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
if grep -R "Company.Template\|__DB_PROVIDER__" . \
  --exclude-dir=bin \
  --exclude-dir=obj \
  --exclude-dir=.git; then
  echo "ERROR: Unresolved template placeholders found."
  exit 1
fi

echo "==> Listing provider-specific files"
find . -name "*DatabaseProviderConfigurator*.cs" -print
find . -name "TestDatabase*.cs" -print

echo "==> Restore"
dotnet restore

echo "==> Build"
dotnet build --no-restore

echo "==> Template test succeeded"
#!/usr/bin/env bash
set -euo pipefail

ZIP_NAME="cleanapi-template.zip"

# Always run from the directory where this script is located.
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$REPO_ROOT"

rm -f "$ZIP_NAME"

zip -r "$ZIP_NAME" . \
  -x "$ZIP_NAME" \
  -x "./.git/*" \
  -x "./.idea/*" \
  -x "./.vs/*" \
  -x "./.vscode/*" \
  -x "./**/bin/*" \
  -x "./**/obj/*" \
  -x "./**/.idea/*" \
  -x "./**/.vs/*" \
  -x "./**/.vscode/*" \
  -x "./**/TestResults/*" \
  -x "./**/.pytest_cache/*" \
  -x "./**/.DS_Store" \
  -x "./**/Thumbs.db" \
  -x "./**/*.user" \
  -x "./**/*.suo"

echo "Created $REPO_ROOT/$ZIP_NAME"

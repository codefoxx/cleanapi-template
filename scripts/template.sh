#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

source "$SCRIPT_DIR/template/common.sh"
source "$SCRIPT_DIR/template/dotnet-tools.sh"
source "$SCRIPT_DIR/template/package.sh"
source "$SCRIPT_DIR/template/content-validation.sh"
source "$SCRIPT_DIR/template/generated-project.sh"
source "$SCRIPT_DIR/template/provider-materialization.sh"
source "$SCRIPT_DIR/template/auth-materialization.sh"
source "$SCRIPT_DIR/template/commands.sh"

main "$@"

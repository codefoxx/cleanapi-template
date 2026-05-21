TOOL_MANIFEST="$REPO_ROOT/.config/dotnet-tools.json"
DOTNET_EF_MODE=""

restore_dotnet_tools() {
  if [[ -f "$TOOL_MANIFEST" ]]; then
    echo "==> Restoring local .NET tools"

    dotnet tool restore --tool-manifest "$TOOL_MANIFEST"
    DOTNET_EF_MODE="local"
    return
  fi

  if dotnet ef --version >/dev/null 2>&1; then
    DOTNET_EF_MODE="global"
    return
  fi

  echo "ERROR: dotnet-ef is not available."
  echo
  echo "Install it globally:"
  echo "  dotnet tool install --global dotnet-ef"
  echo
  echo "Or add a local tool manifest to the repository:"
  echo "  dotnet new tool-manifest"
  echo "  dotnet tool install dotnet-ef"
  exit 1
}

run_dotnet_ef() {
  if [[ -z "$DOTNET_EF_MODE" ]]; then
    restore_dotnet_tools
  fi

  case "$DOTNET_EF_MODE" in
    local)
      (
        cd "$REPO_ROOT"
        dotnet tool run dotnet-ef -- "$@"
      )
      ;;
    global)
      dotnet ef "$@"
      ;;
    *)
      echo "ERROR: Unknown dotnet-ef mode: $DOTNET_EF_MODE"
      exit 1
      ;;
  esac
}

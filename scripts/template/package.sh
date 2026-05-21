pack_template() {
  echo "==> Packing template"

  dotnet msbuild "$TEMPLATE_PROJECT" \
    /target:Pack \
    /property:Configuration="$CONFIGURATION" \
    /terminalLogger:off \
    /verbosity:minimal
}

install_template() {
  echo "==> Installing template"

  dotnet msbuild "$TEMPLATE_PROJECT" \
    /target:InstallTemplate \
    /property:Configuration="$CONFIGURATION" \
    /terminalLogger:off \
    /verbosity:minimal
}

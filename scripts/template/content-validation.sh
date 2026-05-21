build_template_content() {
  echo "==> Restoring raw template content"

  dotnet restore "$REPO_ROOT/content/Company.Template.slnx" \
    /p:EffectiveDbProvider=PostgreSql

  echo "==> Building raw template content"

  dotnet build "$REPO_ROOT/content/Company.Template.slnx" \
    --configuration "$CONFIGURATION" \
    --no-restore \
    /p:EffectiveDbProvider=PostgreSql
}

test_template_content() {
  echo "==> Testing raw template content"

  dotnet test "$REPO_ROOT/content/Company.Template.slnx" \
    --configuration "$CONFIGURATION" \
    --no-build \
    /p:EffectiveDbProvider=PostgreSql
}

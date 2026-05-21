validate_auth_materialization() {
  echo "==> Validating auth materialization"

  local none_project
  local keycloak_project

  none_project="$(auth_materialization_dir none)"
  keycloak_project="$(auth_materialization_dir keycloak)"

  materialize_project "Acme.NoAuth" "PostgreSql" "None" "$none_project"
  materialize_project "Acme.Keycloak" "PostgreSql" "Keycloak" "$keycloak_project"

  assert_no_auth_excludes_auth_artifacts "$none_project" "Acme.NoAuth"
  assert_no_auth_has_no_keycloak_leftovers "$none_project" "Acme.NoAuth"
  assert_keycloak_includes_auth_artifacts "$keycloak_project" "Acme.Keycloak"
  assert_keycloak_apphost_wiring "$keycloak_project" "Acme.Keycloak"
  assert_old_auth_runtime_switches_absent "$keycloak_project" "Acme.Keycloak"
  assert_source_variant_leftovers_absent "$none_project" "$keycloak_project"

  echo "==> Building no-auth generated project"
  dotnet build "$none_project/Acme.NoAuth.slnx"

  echo "==> Building Keycloak generated project"
  dotnet build "$keycloak_project/Acme.Keycloak.slnx"

  echo "==> Auth materialization validation succeeded"
}

assert_no_auth_excludes_auth_artifacts() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking no-auth excludes auth artifacts"

  test ! -d "$project/infra"
  test ! -f "$project/src/$project_name.Api/ApiAdapterServiceModule.cs"
  test ! -f "$project/src/$project_name.Application/Abstractions/Security/ICurrentUser.cs"
  test ! -f "$project/src/$project_name.Api/CurrentUser/HttpCurrentUser.cs"
  test ! -f "$project/src/$project_name.Api/Options/AuthenticationOptions.cs"
  test ! -f "$project/src/$project_name.Api/Security/AuthenticationExtensions.cs"
  test ! -f "$project/src/$project_name.Api/Security/TemplatePolicies.cs"
  test ! -f "$project/src/$project_name.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs"
}

assert_no_auth_has_no_keycloak_leftovers() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking no-auth has no Keycloak/Auth leftovers"

  if grep -RInE 'Keycloak|keycloak|StartKeycloak|__KEYCLOAK|__AUTH_AUDIENCE__|Authentication__' \
    "$project/src/aspire/$project_name.AppHost"; then
    echo "ERROR: no-auth AppHost contains Keycloak/Auth leftovers."
    exit 1
  fi
}

assert_keycloak_includes_auth_artifacts() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking Keycloak includes auth artifacts"

  assert_file_exists "$project/infra/keycloak/realms/acme-keycloak-realm.json"
  assert_file_exists "$project/src/$project_name.Api/ApiAdapterServiceModule.cs"
  assert_file_exists "$project/src/$project_name.Application/Abstractions/Security/ICurrentUser.cs"
  assert_file_exists "$project/src/$project_name.Api/CurrentUser/HttpCurrentUser.cs"
  assert_file_exists "$project/src/$project_name.Api/Options/AuthenticationOptions.cs"
  assert_file_exists "$project/src/$project_name.Api/Security/AuthenticationExtensions.cs"
  assert_file_exists "$project/src/$project_name.Api/Security/TemplatePolicies.cs"
  assert_file_exists "$project/src/$project_name.Api/OpenApi/OAuth2SecuritySchemeTransformer.cs"
}

assert_keycloak_apphost_wiring() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking Keycloak AppHost wiring"

  grep -RInE 'AddTemplateKeycloak|WithTemplateKeycloakAuthentication|Authentication__Authority|Authentication__Audience|acme-keycloak' \
    "$project/src/aspire/$project_name.AppHost" \
    "$project/infra"
}

assert_old_auth_runtime_switches_absent() {
  local project="$1"
  local project_name="$2"

  echo "==> Checking old auth runtime switches are absent"

  if grep -RInE 'StartKeycloak|Authentication__Enabled' \
    "$project/src/aspire/$project_name.AppHost" \
    "$project/infra"; then
    echo "ERROR: old auth runtime switch leftovers found."
    exit 1
  fi
}

assert_source_variant_leftovers_absent() {
  local none_project="$1"
  local keycloak_project="$2"

  echo "==> Checking source variant leftovers are absent"

  if find "$none_project" "$keycloak_project" \
    \( -name '*.None.cs' -o -name '*.PostgreSql.cs' -o -name '*.SqlServer.cs' \) \
    -print | grep -q .; then
    echo "ERROR: source variant leftovers found."
    exit 1
  fi
}

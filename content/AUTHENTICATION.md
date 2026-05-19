# AUTHENTICATION

> Optional local Keycloak authentication for development and smoke testing.

## Default behavior

Authentication is disabled by default:

```json
{
    "Authentication": {
        "Enabled": false
    }
}
```

Enable local Keycloak orchestration in the AppHost:

```json
{
    "AppHost": {
        "StartKeycloak": true
    }
}
```

When Keycloak is started by Aspire, the AppHost:

- starts a local Keycloak container
- imports the prepared local development realm
- wires the API authentication settings
- configures the API authority and audience
- keeps authentication optional through configuration

The API validates bearer tokens. It does not perform browser login and does not use cookie authentication.

## Local development realm

The local Keycloak realm import lives in:

```text
infra/keycloak/realms/
```

The template generates the realm file name from the project name.

Example for:

```bash
dotnet new cleanapi -n Acme.Products --db PostgreSql
```

the generated realm file is:

```text
infra/keycloak/realms/acme-products-realm.json
```

The realm name is:

```text
acme-products
```

The file name must match the Keycloak import convention:

```text
<realm-name>-realm.json
```

So for realm `acme-products`, the file must be named:

```text
acme-products-realm.json
```

Keycloak rejects the import if the file name and realm name do not match.

## Local API client

The imported realm contains a local development API client:

```text
Client ID: acme-products-api
Client secret: local-dev-secret
Flow: client_credentials
```

The client is intended for local development and smoke tests only.

Do not reuse the local development client secret in real environments.

The generated API expects:

```text
Authority: http://localhost:8080/realms/acme-products
Audience:  acme-products-api
```

The access token must contain:

```json
{
    "iss": "http://localhost:8080/realms/acme-products",
    "aud": [
        "acme-products-api",
        "account"
    ],
    "scope": "profile products.read products.write email"
}
```

The important parts are:

| Claim | Requirement |
| --- | --- |
| `iss` | Must exactly match the API Authority. |
| `aud` | Must contain the API audience. |
| `scope` | Must contain the scopes required by the endpoint. |

JWT issuer validation is strict. The issuer must match exactly, including port and casing.

For example, these are different issuers:

```text
http://localhost:8080/realms/acme-products
http://localhost:8080/realms/Acme-Products
http://localhost:32804/realms/acme-products
```

## Authorization scopes

The template uses OAuth scopes for API authorization.

The local realm includes:

```text
products.read
products.write
```

The sample policies are:

| Scope | Meaning |
| --- | --- |
| `products.read` | Allows reading product data. |
| `products.write` | Allows creating and modifying product data. |

The local development client receives both scopes by default.

## Token request

After starting the AppHost with Keycloak enabled, request a token:

```bash
KC_URL="http://localhost:8080"
REALM="acme-products"
CLIENT_ID="acme-products-api"
CLIENT_SECRET="local-dev-secret"

TOKEN_RESPONSE=$(curl -s -X POST "$KC_URL/realms/$REALM/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=$CLIENT_ID" \
  -d "client_secret=$CLIENT_SECRET")

TOKEN=$(echo "$TOKEN_RESPONSE" | jq -r '.access_token')
```

Decode the token payload:

```bash
echo "$TOKEN" | cut -d "." -f2 | base64 -d 2>/dev/null | jq
```

If the API returns:

```text
401 Unauthorized
WWW-Authenticate: Bearer error="invalid_token", error_description="The issuer '...' is invalid"
```

then the token issuer does not match the API `Authentication:Authority`.

Common causes:

- token was requested from the wrong Keycloak port
- realm casing differs
- API was started with stale authentication settings
- Keycloak was started with an old persisted volume

## Using Insomnia

When authentication is enabled, the OpenAPI document includes OAuth2 client-credentials metadata for secured endpoints.

After importing `/openapi/v1.json`, Insomnia should be able to use the OAuth2 metadata to request tokens for protected endpoints.

Use these local development values:

```text
Grant type: Client Credentials
Access Token URL: http://localhost:8080/realms/acme-products/protocol/openid-connect/token
Client ID: acme-products-api
Client Secret: local-dev-secret
Scope: products.read products.write
```

The token must be requested from the same Keycloak URL that the API uses as `Authentication:Authority`.

Otherwise, JWT validation fails with an invalid issuer error.

If Insomnia only shows a Bearer Token field, request the token manually with `curl` and paste the access token into the Bearer Token value.

## API smoke test

The template includes a unified k6 smoke test for the sample API.

In the default `AUTH_MODE=none` mode, the script verifies:

- invalid application requests return expected errors
- all sample Product endpoints work end-to-end

When `AUTH_MODE=keycloak` is set, the same script also verifies:

- Keycloak token retrieval through `client_credentials`
- token issuer
- token audience
- `products.read` and `products.write` scopes
- unauthenticated requests return `401`

The script lives in:

```text
scripts/smoke/api-smoke.js
```

Install k6 first:

```bash
sudo apt update
sudo apt install -y gpg ca-certificates

curl -fsSL https://dl.k6.io/key.gpg | gpg --dearmor | sudo tee /usr/share/keyrings/k6-archive-keyring.gpg > /dev/null

echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list

sudo apt update
sudo apt install -y k6
```

Run the default smoke test without Keycloak:

```bash
k6 run scripts/smoke/api-smoke.js
```

`AUTH_MODE=none` is the default quick smoke path.

To test the Keycloak-authenticated mode, start the AppHost with Keycloak enabled:

```json
{
    "AppHost": {
        "StartKeycloak": true
    }
}
```

Then run:

```bash
AUTH_MODE=keycloak \
KC_URL="http://localhost:8080" \
API_URL="http://localhost:5080" \
KC_REALM="acme-products" \
KC_CLIENT_ID="acme-products-api" \
KC_CLIENT_SECRET="local-dev-secret" \
k6 run scripts/smoke/api-smoke.js
```

Adjust `API_URL` if the API runs on a different local port.

Expected result:

```text
checks: 100%
http_req_failed: 0%
```

The smoke test intentionally treats some HTTP errors as expected responses:

| Status | Meaning |
| --- | --- |
| `400` | malformed request or request binding failure |
| `401` | unauthenticated request |
| `404` | product not found |
| `422` | invalid application request rejected by application validation |

A `403` is not treated as expected in the full-access smoke test.

If the smoke test returns `403`, the token was accepted but the API policy or scopes do not match.

---

## Related documents

- [README](README.md)
- [ARCHITECTURE](ARCHITECTURE.md)
- [APPLICATION](APPLICATION.md)
- [API](API.md)
- [RESULTS](RESULTS.md)
- [PERSISTENCE](PERSISTENCE.md)
- [OBSERVABILITY](OBSERVABILITY.md)
- [DATABASE](DATABASE.md)
- [ASPIRE](ASPIRE.md)
- [AUTHENTICATION](AUTHENTICATION.md)
- [OPENAPI](OPENAPI.md)
- [MIGRATIONS](MIGRATIONS.md)
- [TESTING](TESTING.md)
- [FEATURES](FEATURES.md)

# OPENAPI

> OpenAPI metadata for local development and API tooling.

## Document URL

In development:

```text
/openapi/v1.json
```

When authentication is disabled, the OpenAPI document contains no authentication metadata.

When authentication is enabled, the OpenAPI document includes OAuth2 client-credentials metadata for secured endpoints.

## Authentication metadata

The generated OpenAPI document contains:

- the Keycloak token endpoint
- the required OAuth scopes
- `401` and `403` responses for protected endpoints
- per-operation security requirements

Example security scheme:

```json
{
    "type": "oauth2",
    "flows": {
        "clientCredentials": {
            "tokenUrl": "http://localhost:8080/realms/acme-products/protocol/openid-connect/token",
            "scopes": {
                "products.read": "Allows reading product data.",
                "products.write": "Allows creating and modifying product data."
            }
        }
    }
}
```

## API tool setup

Tools such as Insomnia can import the OpenAPI document and configure OAuth2 authentication for protected requests.

For the local development realm, use:

```text
Grant type: Client Credentials
Token URL:  http://localhost:8080/realms/acme-products/protocol/openid-connect/token
Client ID:  acme-products-api
Secret:     local-dev-secret
Scopes:     products.read products.write
```

If your API tool only shows a Bearer Token field, request a token manually and paste the access token into that field.

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

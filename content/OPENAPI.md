# OPENAPI

> OpenAPI metadata for local development and API tooling.

## Document URL

In development:

```text
/openapi/v1.json
```

When authentication is disabled, the OpenAPI document contains no authentication metadata.

When authentication is enabled, the OpenAPI document includes OAuth2 client-credentials metadata for secured endpoints.

## Endpoint metadata

Endpoint modules should advertise expected responses explicitly.

The sample Product endpoints document:

- success responses such as `200`, `201`, and `204`
- `400` for ASP.NET Core request binding or malformed request failures
- `422` for application/request validation failures
- `404` for missing resources
- `409` for lifecycle or state conflicts

Route parameter names in OpenAPI come from the Minimal API handler parameter names. Prefer descriptive names such as `productId` when that makes the API document clearer:

```text
/api/products/{productId}
/api/products/{productId}/price
/api/products/{productId}/discontinue
```

Keep OpenAPI tests aligned with the generated document.

## Validation metadata

Validation responses use `application/problem+json` and `HttpValidationProblemDetails`.

Request validation that is handled by the API validation builder returns `422 Unprocessable Entity` with field-level errors:

```json
{
    "title": "Validation failed.",
    "status": 422,
    "detail": "One or more validation errors occurred.",
    "code": "validation_error",
    "errors": {
        "name": ["Product name is required."]
    }
}
```

`400 Bad Request` remains documented because the ASP.NET Core pipeline can still reject malformed JSON, invalid binding, or other syntactic request problems before application validation runs.

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

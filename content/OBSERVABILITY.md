# OBSERVABILITY

> OpenTelemetry and structured logging are first-class production concerns.

## What is configured

Service defaults configure:

- OpenTelemetry logging
- OpenTelemetry tracing
- OpenTelemetry metrics
- ASP.NET Core instrumentation
- HTTP client instrumentation
- runtime instrumentation
- OTLP export when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured

Application use cases are decorated with telemetry behavior.

The use-case telemetry decorator records:

- use-case start
- use-case completion
- use-case failure
- use-case duration
- unexpected exceptions
- cancellation
- OpenTelemetry spans
- OpenTelemetry metrics
- structured logs

## File locations

Generic execution telemetry belongs in:

```text
src/Company.Template.Application/Telemetry/
```

Application telemetry definitions belong in:

```text
src/Company.Template.Application/Diagnostics/
```

Feature-specific business logs should live with the feature, for example:

```text
src/Company.Template.Application/Products/
```

## Logging rules

- Use structured logging.
- Do not use string interpolation in log messages.
- Use logs to explain application decisions, not every method call.
- Use OpenTelemetry traces to understand execution flow.
- Use metrics for rates, counts, and durations.
- Do not put high-cardinality values such as product IDs, user emails, request IDs, or exception messages into metric tags.
- Log unexpected exceptions once at the boundary or in cross-cutting telemetry.
- Expected failures should normally be represented as `Result` values.

## Telemetry signal roles

| Signal | Role |
| --- | --- |
| Logs | Explain what the application decided and why. |
| Traces | Show request, use-case, dependency, and database flow. |
| Metrics | Show rates, counts, failures, and duration distributions. |

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

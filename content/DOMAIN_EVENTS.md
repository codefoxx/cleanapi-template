# Domain Events

This template includes a small in-process domain event mechanism to demonstrate how aggregates can record facts that happened during domain operations.

The mechanism is intentionally simple and should be treated as a learning/example implementation, not as a production-ready durable event delivery system.

## How it works

Aggregates inherit from `AggregateRoot` and can record domain events while executing domain behavior.

`ApplicationDbContext.SaveChangesAsync(...)` collects recorded events from tracked aggregates, saves the EF Core changes, and then dispatches the collected events through `IDomainEventDispatcher`.

After the save cycle, recorded events are cleared from the tracked aggregate instances.

## Important limitations

This mechanism dispatches events after the database save has completed.

That means:

- the database change may already be committed when event dispatch fails
- a dispatch failure is returned to the caller even though the state change was persisted
- events are not stored durably
- failed events are not retried
- duplicate handling, ordering guarantees, and poison-message handling are not provided
- external side effects such as emails, webhooks, message publishing, or cross-service communication should not rely on this mechanism

This is suitable only for local, in-process, best-effort reactions where losing or duplicating a reaction would not break the business workflow.

## Production guidance

For production-grade event delivery, use a transactional outbox or a messaging framework that supports durable inbox/outbox processing.

Common options in the .NET ecosystem include:

- MassTransit with EF Core Outbox
- Wolverine with EF Core-backed durability
- NServiceBus with Outbox support

Those tools solve concerns that this template intentionally does not implement by default, such as durable storage, retries, concurrency, duplicate handling, and operational monitoring.

## Why this template does not include an Outbox by default

An Outbox is the right pattern when domain events drive external side effects or integration events.

However, a correct production-grade Outbox adds additional concepts:

- outbox message storage
- serialization and versioning
- background processing
- retry policies
- idempotent handlers
- poison-message handling
- cleanup
- monitoring

Including all of that by default would make the base template much harder to understand.

The current implementation exists to teach the dependency direction and aggregate event recording pattern. Applications that need reliable event delivery should replace or extend this mechanism with a proper Outbox implementation.
# Roadmap note: Product SKU as natural key

## Goal

Add a future story for extending the `Products` sample with a SKU value object and using that SKU as the business-facing natural key.

The aggregate should still keep its technical identity, for example a GUID-based `ProductId`, while the SKU becomes the domain key that represents the product in business terms.

## Why

The template should demonstrate that technical identity and business identity are different concerns.

A relational model can support both:

- a surrogate primary key for technical identity and references,
- a natural business key for domain uniqueness and lookup.

EF Core supports this model through alternate keys, for example with `HasAlternateKey`.

## Future story scope

A later story should evaluate and implement:

- add a `ProductSku` value object,
- make SKU required for `Product`,
- keep `ProductId` as the aggregate identity,
- configure SKU as an EF Core alternate key,
- enforce SKU uniqueness at the database level,
- update create-product request validation,
- update product persistence mapping,
- add domain and persistence tests for SKU uniqueness,
- decide explicitly which API operations should use `ProductId` and which should use SKU.

## Design note

The first implementation should not blindly replace every route identifier with SKU.

That decision belongs to the API design story. The important baseline is to show that the domain model can have a stable business key while the persistence model still keeps a technical primary key.

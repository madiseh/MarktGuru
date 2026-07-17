# Architecture

## Overview

Offers Service is an ASP.NET Core backend for retail product offers. It exposes HTTP endpoints, stores data with Entity Framework Core, and uses Hangfire for background processing.

## Layers

| Layer | Location | Responsibility |
|-------|----------|----------------|
| API | `src/OffersService/Controllers` | Request routing, validation responses, HTTP status codes |
| Services | `src/OffersService/Services` | Business logic, imports, offer/product workflows |
| IOC | `src/OffersService/IOC` | Dependency injection and application middleware registration extensions |
| Validators | `src/OffersService/Validators` | FluentValidation rules for API/import contracts |
| Exceptions | `src/OffersService/Exceptions` | Problem-details exception handling for validation and unexpected errors |
| Extensions | `src/OffersService/Extensions` | Shared query and framework helper extensions |
| Models | `src/OffersService/Models` | EF Core entities and domain enums |
| DTOs | `src/OffersService/DTOs` | API and import contracts |
| Data | `src/OffersService/Data` | DbContext, entity type configurations, migrations |
| Tests | `tests/OffersService.Tests` | Unit and integration coverage |

## Application Composition

`Program.cs` wires top-level application startup only. Feature-specific service
registrations and middleware setup live behind extension methods in
`src/OffersService/IOC`, including application services and centralized
exception handling.

## Import Validation

Offer imports are validated before persistence with FluentValidation. Row-level
rules validate product ID, price, and validity dates. Request-level rules check
that referenced products exist and return structured `400 Bad Request` problem
details when validation fails.

Validator error messages must be centralized in a focused message catalog next
to the validator, for example `ImportValidationMessages` in
`src/OffersService/Validators`. Validators and tests should reference those
constants or factory methods instead of repeating literal error strings inline.

## Data Model

Current seeded model:

- `Product`: product records.
- `Retailer`: retailer master data with unique name, country, active flag, and creation time.
- `Offer`: offer records with price, validity window, status, and optional retailer relationship.
- `OfferStatus`: `Pending`, `Active`, `Expired`.

Entity mapping and seed data are defined in `src/OffersService/Data/Configurations` using EF Core `IEntityTypeConfiguration<T>` classes and loaded from `AppDbContext.OnModelCreating`. Every entity configuration must explicitly set the table name with `ToTable(...)`, and table names must come from `src/OffersService/Data/TableNames.cs`.

## Query Guidelines

- Prefer projections for read endpoints that join multiple tables.
- Avoid N+1 queries by using projections, includes, or aggregate queries intentionally.
- For paginated endpoints, compute `total` before pagination is applied.
- Requirements that specify a single database round-trip should be handled with one composed EF query or a deliberate raw SQL query.

## Background Jobs

Planned job:

- Name: `OfferExpiryJob`.
- Schedule: every hour through Hangfire cron registration.
- Behavior: expire offers where `ValidTo < UTC now` and `Status != Expired`.
- Database behavior: one bulk update statement.
- Operational behavior: idempotent, async, cancellation-token aware, and logs the affected count.

## Documentation Rule

If architecture changes, update this file and append a short note to `AGENTS.md`.

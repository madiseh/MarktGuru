# Architecture

## Overview

Offers Service is an ASP.NET Core backend for retail product offers. It exposes HTTP endpoints, stores data with Entity Framework Core, and uses Hangfire for background processing.

## Layers

| Layer | Location | Responsibility |
|-------|----------|----------------|
| API | `src/OffersService/Controllers` | Request routing, validation responses, HTTP status codes |
| Services | `src/OffersService/Services` | Business logic, imports, offer/product workflows |
| Models | `src/OffersService/Models` | EF Core entities and domain enums |
| DTOs | `src/OffersService/DTOs` | API and import contracts |
| Data | `src/OffersService/Data` | DbContext, model configuration, migrations |
| Tests | `tests/OffersService.Tests` | Unit and integration coverage |

## Data Model

Current seeded model:

- `Product`: product records.
- `Offer`: offer records with price, validity window, and status.
- `OfferStatus`: `Pending`, `Active`, `Expired`.

Planned model extension from `README.md`:

- `Retailer`: retailer master data with unique name, country, active flag, and creation time.
- `Offer.RetailerId`: nullable foreign key to `Retailer`.

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

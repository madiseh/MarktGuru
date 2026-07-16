# Solution

## Bugs Found and Fixed

- 2026-07-16: Awaited offer import execution from `OffersController` and changed the import service contract from `async void` to `Task`.
- 2026-07-16: Returned the imported offer row count from the import service and `POST /api/offers/import` response body.
- 2026-07-16: test fix so active-offer seed expectations are computed dynamically.
- 2026-07-16: Added FluentValidation import validation so invalid offer rows and missing product IDs return `400 Bad Request` before persistence.

## Design Decisions

- 2026-07-16: Added centralized problem-details exception handlers for validation and unexpected application errors, registered through `src/OffersService/IOC/ExceptionHandling.cs`.
- 2026-07-16: Switched integration tests to SQLite in-memory with foreign keys enabled so relational constraints are exercised.
- 2026-07-16: Added `src/OffersService/IOC/Services.cs` for application service dependency injection registration.
- 2026-07-16: Added `TableNames` constants for EF Core table names and updated configurations to use them.
- 2026-07-16: Required explicit EF Core table names and added `ToTable(...)` to current entity configurations.
- 2026-07-16: Moved EF Core entity mapping and seed data into `Data/Configurations` with `IEntityTypeConfiguration<T>`.
- 2026-07-16: Added project agent rules, documentation routing, and initial `docs/` Markdown files.

## With More Time

- To be completed before submission.

## Time Spent

- Without AI: To be completed before submission.
- With AI: To be completed before submission.

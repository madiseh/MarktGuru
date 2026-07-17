# Solution

## Bugs Found and Fixed

- 2026-07-16: Awaited offer import execution from `OffersController` and changed the import service contract from `async void` to `Task`.
- 2026-07-16: Returned the imported offer row count from the import service and `POST /api/offers/import` response body.
- 2026-07-16: test fix so active-offer seed expectations are computed dynamically.
- 2026-07-16: Added FluentValidation import validation so invalid offer rows and missing product IDs return `400 Bad Request` before persistence.
- 2026-07-16: Fixed active-offer filtering so future offers are excluded until their `ValidFrom` date is reached.
- 2026-07-16: Replaced the blocking active-offers endpoint `.Result` call with async/await and added integration coverage for the response body.
- 2026-07-17: Added validation for null import row payloads so `rows = null` returns `400 Bad Request` instead of being enumerated by the import service.
- 2026-07-17: Added validation for null entries inside import rows so `rows = [null]` returns `400 Bad Request` with a row-level error instead of dereferencing the row.
- 2026-07-17: Asserting the missing-product validation response body in import integration tests.
- 2026-07-17: Eliminated the products endpoint N+1 offer loading query and added product service/API regression tests.
- 2026-07-17: Added validated `GET /api/offers` listing with pagination, filtering, and integration coverage for invalid query values.
- 2026-07-17: Reworked `GET /api/offers` pagination to return page rows and the unpaged total in one database command, and tightened command-count coverage to require one round-trip.
- 2026-07-17: Moved repeated UTC `DateTime` kind normalization into a shared `DateTimeExtensions.EnsureUtc` helper.
- 2026-07-17: Matched the offer-list response contract by renaming the pagination total property to `Total`.
- 2026-07-17: Added `GET /api/retailers/{id}/stats` with route id validation, active-retailer 404 behavior, single-query active-offer aggregates, and unit/integration coverage.
- 2026-07-17: Added `POST /api/retailers` with centralized validation, duplicate-name conflict handling, `201 Created` responses, and integration coverage for success, duplicate names, and invalid country codes.
- 2026-07-17: Tightened retailer country validation from two alphabetic characters to membership in officially assigned ISO 3166-1 alpha-2 country codes.
- 2026-07-17: Updated `GET /api/offers` to include offers without retailers by returning nullable retailer fields instead of filtering those rows out.

## Design Decisions

- 2026-07-17: Added the `Retailers` table with required unique names, two-character country codes, active status, creation time, and seeded `Rewe` and `Billa` records.
- 2026-07-17: Added nullable `Offer.RetailerId` support with an EF Core navigation to `Retailer`, an index on `Offers.RetailerId`, and `SetNull` delete behavior so offers are retained if a retailer is removed.
- 2026-07-17: Updated existing seeded offers to reference the seeded retailers while keeping `RetailerId` nullable as required by the exercise.
- 2026-07-17: Kept retailer mapping in `RetailerConfiguration` and table naming in `TableNames` to match the project's configuration-based EF Core structure.
- 2026-07-16: Added centralized problem-details exception handlers for validation and unexpected application errors, registered through `src/OffersService/IOC/ExceptionHandling.cs`.
- 2026-07-16: Switched integration tests to SQLite in-memory with foreign keys enabled so relational constraints are exercised.
- 2026-07-16: Added `src/OffersService/IOC/Services.cs` for application service dependency injection registration.
- 2026-07-16: Added `TableNames` constants for EF Core table names and updated configurations to use them.
- 2026-07-16: Required explicit EF Core table names and added `ToTable(...)` to current entity configurations.
- 2026-07-16: Moved EF Core entity mapping and seed data into `Data/Configurations` with `IEntityTypeConfiguration<T>`.
- 2026-07-16: Added project agent rules, documentation routing, and initial `docs/` Markdown files.
- 2026-07-17: Centralized import validation messages in `ImportValidationMessages` so validators and tests do not duplicate literal error text.
- 2026-07-17: Registered offer-list validation through `src/OffersService/IOC/Services.cs` to preserve the current application composition pattern.
- 2026-07-17: Introduced a reusable `PagedList<T>` response model with page metadata and navigation flags for offer listing responses.
- 2026-07-17: Kept offer listing in `OfferService` with EF Core LINQ only, avoiding hand-written SQL while the list endpoint still uses one database command.
- 2026-07-17: Registered retailer stats services and validators through `src/OffersService/IOC/Services.cs` to keep application composition centralized.
- 2026-07-17: Added `GET /api/retailers/{id}` so retailer creation responses can point the `Location` header at a concrete retailer resource.
- 2026-07-17: Added `OfferExpiryJob` with bulk EF Core expiration logic and registered jobs through a focused `src/OffersService/IOC/Jobs.cs` extension.
- 2026-07-17: Registered Hangfire recurring execution for `OfferExpiryJob` in `Program.cs` with the hourly cron expression helper.
- 2026-07-17: Added `Allow User Variables=true` to MySQL connection strings so Hangfire dashboard queries that use `@rownum` work with MySQL storage.
- 2026-07-17: Added SQLite-backed unit tests for `OfferExpiryJob` covering matching-row expiration, clean-database idempotency, logging, and one database `UPDATE` command.
- 2026-07-17: Moved the test `ListLogger<T>` into `tests/OffersService.Tests/Helpers` so future tests can reuse captured log assertions.
- 2026-07-17: Moved Hangfire recurring job registration into `src/OffersService/IOC/RecurringJobs.cs` and kept `Program.cs` as a single extension-method call.

## With More Time

- To be completed before submission.

## Time Spent

- Without AI: To be completed before submission.
- With AI: To be completed before submission.

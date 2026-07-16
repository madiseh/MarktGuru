# Project Structure

## Root

```text
.
|-- README.md
|-- AGENTS.md
|-- OffersService.sln
|-- docker-compose.yml
|-- .env.example
|-- src/
|-- tests/
`-- docs/
```

## Service

```text
src/OffersService/
|-- Controllers/
|   |-- OffersController.cs
|   `-- ProductsController.cs
|-- Services/
|   |-- OfferService.cs
|   |-- ProductService.cs
|   `-- OfferImportService.cs
|-- Models/
|   |-- Product.cs
|   |-- Offer.cs
|   `-- OfferStatus.cs
|-- DTOs/
`-- Data/
    |-- AppDbContext.cs
    |-- Configurations/
    |   |-- ProductConfiguration.cs
    |   `-- OfferConfiguration.cs
    `-- Migrations/
```

## Tests

```text
tests/OffersService.Tests/
|-- Unit/
|   `-- OfferServiceTests.cs
|-- Integration/
|   `-- OffersControllerTests.cs
|-- Helpers/
|   `-- TestWebApplicationFactory.cs
`-- OffersService.Tests.csproj
```

## Current API Surface

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/offers/active` | Returns all currently active offers |
| `GET` | `/api/offers/{id}` | Returns a single offer by ID |
| `POST` | `/api/offers/import` | Bulk-imports offers from a JSON payload |
| `GET` | `/api/products` | Returns products with their associated offers |

## Planned README Tasks

- Fix existing correctness and reliability issues.
- Add `Retailers` schema and migration.
- Add offer listing, retailer stats, and retailer creation endpoints.
- Add an hourly Hangfire `OfferExpiryJob`.
- Expand tests for bug fixes, new endpoints, and the expiry job.

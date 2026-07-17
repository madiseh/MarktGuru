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
|   |-- ProductsController.cs
|   `-- RetailersController.cs
|-- Services/
|   |-- OfferService.cs
|   |-- ProductService.cs
|   |-- OfferImportService.cs
|   `-- RetailerService.cs
|-- Validators/
|   |-- OfferListRequestValidator.cs
|   |-- OfferListValidationMessages.cs
|   |-- ImportRequestValidator.cs
|   |-- OfferRowValidator.cs
|   |-- RetailerStatsRequestValidator.cs
|   `-- RetailerValidationMessages.cs
|-- Exceptions/
|   |-- GlobalExceptionHandler.cs
|   `-- ValidationExceptionHandler.cs
|-- Extensions/
|   `-- QueryableExtensions.cs
|-- Models/
|   |-- Product.cs
|   |-- Offer.cs
|   |-- OfferStatus.cs
|   `-- Retailer.cs
|-- DTOs/
|   |-- OfferListItemDto.cs
|   |-- OfferListRequest.cs
|   |-- OfferListStatus.cs
|   |-- RetailerStatsRequest.cs
|   |-- RetailerStatsDto.cs
|   `-- MostRecentActiveOfferDto.cs
|-- Models/Pagination/
|   `-- PagedList.cs
|-- IOC/
|   |-- ExceptionHandling.cs
|   `-- Services.cs
`-- Data/
    |-- AppDbContext.cs
    |-- Configurations/
    |   |-- ProductConfiguration.cs
    |   |-- OfferConfiguration.cs
    |   `-- RetailerConfiguration.cs
    `-- Migrations/
```

## Tests

```text
tests/OffersService.Tests/
|-- Unit/
|   |-- OfferServiceTests.cs
|   |-- ProductServiceTests.cs
|   `-- RetailerServiceTests.cs
|-- Integration/
|   |-- OffersControllerTests.cs
|   |-- ProductsControllerTests.cs
|   `-- RetailersControllerTests.cs
|-- Helpers/
|   |-- DbCommandCounterInterceptor.cs
|   `-- TestWebApplicationFactory.cs
`-- OffersService.Tests.csproj
```

## Current API Surface

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/offers` | Returns validated, paginated offers filtered by product, retailer, and status |
| `GET` | `/api/offers/active` | Returns all currently active offers |
| `GET` | `/api/offers/{id}` | Returns a single offer by ID |
| `POST` | `/api/offers/import` | Bulk-imports offers from a JSON payload |
| `GET` | `/api/products` | Returns products with their associated offers |
| `GET` | `/api/retailers/{id}/stats` | Returns single-query active-offer statistics for an active retailer |

## README Tasks

- Fixed existing correctness and reliability issues.
- Added `Retailers` schema and migration.
- Add offer listing, retailer stats, and retailer creation endpoints.
- Add an hourly Hangfire `OfferExpiryJob`.
- Expand tests for bug fixes, new endpoints, and the expiry job.

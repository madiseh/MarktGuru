# Offers Service — Backend Exercise

You are joining the backend team for a day. This is a real ASP.NET Core service that manages retail product offers — the kind of data that powers the marktguru platform. Your job is to fix existing issues, extend the schema, build new endpoints, set up a background job, and keep the test suite honest throughout.

---

## Tech stack

| | |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| ORM | Entity Framework Core 9 (Pomelo MySQL provider) |
| Database | MariaDB 10.11 (via Docker) |
| Background jobs | Hangfire 1.8 |
| Tests | xUnit · Moq · Microsoft.AspNetCore.Mvc.Testing |

---

## Setup

### 1. Start the database

```bash
docker compose up -d
```

Wait for the `db` container to become healthy (usually 20–30 seconds):

```bash
docker compose ps
```

### 2. Apply migrations

From the `exercise/` directory (where this README is):

```bash
dotnet ef database update --project src/OffersService
```

### 3. Run the service

```bash
dotnet run --project src/OffersService
```

The API is available at `http://localhost:5000`.  
OpenAPI schema: `http://localhost:5000/openapi/v1.json`  
Hangfire dashboard: `http://localhost:5000/hangfire`

### 4. Run the tests

```bash
dotnet test
```

The test suite currently has **5 tests: 2 pass, 3 fail**. Fixing the failures is part of the exercise.

---

## What's in the codebase

```
src/OffersService/
├── Controllers/
│   ├── OffersController.cs       ← three existing endpoints
│   └── ProductsController.cs
├── Services/
│   ├── OfferService.cs
│   ├── ProductService.cs
│   └── OfferImportService.cs
├── Models/
│   ├── Product.cs
│   ├── Offer.cs
│   └── OfferStatus.cs            ← Pending / Active / Expired
├── DTOs/
└── Data/
    ├── AppDbContext.cs
    └── Migrations/
```

### Existing endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/offers/active` | Returns all currently active offers |
| `GET` | `/api/offers/{id}` | Returns a single offer by ID |
| `POST` | `/api/offers/import` | Bulk-imports offers from a JSON payload |
| `GET` | `/api/products` | Returns products with their associated offers |

### Seed data

The initial migration seeds the database with 3 products and 3 offers. One of the seeded offers has a `ValidTo` date in the past.

---

## Tasks

### Task 1 — Fix the existing issues

The codebase has issues that affect correctness and reliability under real usage conditions. Run `dotnet test` to see the test failures — they point to some of the problems, but not all of them. Read the code carefully.

**What good looks like:**
- All failing tests pass after your fixes.
- You can explain *why* each bug existed and what the correct behaviour is.
- You have updated or added tests to cover the fixed code paths.

> The nature of the bugs is intentionally not described here — finding and diagnosing them is part of the exercise.

---

### Task 2 — Extend the schema

Add a `Retailers` table:

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` PK | auto-increment |
| `Name` | `varchar(200)` | required, unique |
| `Country` | `char(2)` | ISO 3166-1 alpha-2 code |
| `IsActive` | `bool` | default `true` |
| `CreatedAt` | `datetime` | |

Add a `RetailerId` foreign key column (nullable) to the `Offers` table.

Write the EF Core migration. It must be reversible — the `Down()` method should cleanly undo the changes. Seed at least two retailers so the app is runnable after migration.

**What good looks like:**
- Migration applies and rolls back cleanly.
- The `Offer` entity correctly navigates to `Retailer`.
- No orphaned foreign key columns left from the old string-based approach.

---

### Task 3 — New API endpoints

#### `GET /api/offers`

Paginated, filterable list of offers. Joins across Products, Offers, and Retailers.

Query parameters:

| Parameter | Type | Default | Notes |
|-----------|------|---------|-------|
| `page` | int | 1 | 1-based |
| `pageSize` | int | 20 | max 100; return 400 if exceeded |
| `productId` | int | — | optional filter |
| `retailerId` | int | — | optional filter |
| `status` | string | `active` | `active`, `expired`, or `all` |

Response shape:

```json
{
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Whole Milk 1L",
      "retailerId": 2,
      "retailerName": "Billa",
      "price": 0.99,
      "validFrom": "2024-01-01T00:00:00Z",
      "validTo": "2099-12-31T00:00:00Z",
      "status": "Active"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20
}
```

The query must not issue more than one database round-trip regardless of result size.

#### `GET /api/retailers/{id}/stats`

Aggregated statistics for a single retailer.

```json
{
  "id": 1,
  "name": "Rewe",
  "country": "AT",
  "isActive": true,
  "activeOfferCount": 42,
  "avgPrice": 1.89,
  "lowestPrice": 0.49,
  "mostRecentActiveOffer": {
    "id": 17,
    "productName": "Whole Milk 1L",
    "price": 0.99,
    "validTo": "2099-12-31T00:00:00Z"
  }
}
```

- Returns `404` if the retailer does not exist or `IsActive = false`.
- Must execute as a **single database round-trip** — loading the most recent offer in a separate query is not acceptable.

#### `POST /api/retailers`

Creates a new retailer.

- Returns `201 Created` with a `Location` header pointing to the new resource.
- Validation: `name` required (max 200 chars), `country` required (exactly 2-char ISO code).
- Returns `409 Conflict` if a retailer with the same name already exists.
- Returns `400 Bad Request` for any validation failure.

**What good looks like:**
- No N+1 queries anywhere. Use EF Core's `Include`, projections, or raw aggregation — your choice, but be ready to justify it.
- Pagination is consistent: `total` reflects the count *before* pagination, not after.
- Invalid inputs return the right status codes, not 500s.

---

### Task 4 — Background job

Register a Hangfire recurring job named `OfferExpiryJob` that runs **every hour**.

The job must:

1. Find all offers where `ValidTo < UTC now` **and** `Status != Expired`
2. Update their `Status` to `Expired` in a **single bulk statement** — not row by row
3. Log: `"Expired {n} offers at {timestamp}"`
4. Be **idempotent** — running it twice in a row must produce the same result
5. Use `async`/`await` correctly throughout
6. Accept and propagate `CancellationToken`

**What good looks like:**
- A single `UPDATE` statement reaches the database, not one per offer.
- The job is registered in `Program.cs` with a cron expression, not a hardcoded interval.
- Running the job against an already-clean database completes without error and logs `"Expired 0 offers"`.

---

## Tests

Update the failing tests as part of Task 1. For the new work, add:

- Tests for `GET /api/offers`: happy path with data, empty result set, invalid `pageSize`.
- Tests for `POST /api/retailers`: success (201 + Location header), duplicate name (409), invalid country code (400).
- A unit test for `OfferExpiryJob`: verify that only offers satisfying *both* conditions (`ValidTo < now` AND `Status != Expired`) are updated, and the return count matches.

A test that only asserts on the HTTP status code without checking the response body does not meet the bar.

---

## Submission

1. Create a Solution.md file where you describe:
   - What bugs you found and how you fixed them
   - Any design decisions that weren't obvious from the requirements
   - Anything you would do differently with more time
   - How much time it took you without AI, and with AI
2. Send the solution as a zip file or share a github repository link
---

## Expectation

Time expectation: **6–8 hours** without AI assistance.

You are welcome — and encouraged — to use AI tools. We will discuss your solution and your process in a follow-up call, so make sure you understand every line you submit.

We expect full honesty in which approach you took, time spent, and why.

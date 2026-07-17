# AGENTS.md

## Core Rules

- Follow `C:\Users\madis\.codex\RTK.md`: run shell commands through `rtk`.
- Keep durable project rules in `AGENTS.md`, relevant long-form guidance under `docs/`, and implementation/change notes in `Solution.md`.
- Keep architecture and other long-form notes in separate Markdown files under `docs/`.
- Before working on a topic, read the matching document from `Documentation Routing`.
- Do not overwrite user changes. Read the current files first and work with the existing state.

## Documentation Routing

- Project structure, folders, or file placement: read `docs/PROJECT_STRUCTURE.md`.
- Architecture, data model, service boundaries, API design, or background jobs: read `docs/ARCHITECTURE.md`.
- Git commits, commit messages, staging, or release notes from commits: read `docs/GIT_COMMIT_GUIDELINES.md`.
- If a new topic needs durable guidance, create a focused Markdown file under `docs/` and add it to this routing list.

## Development Commands

```powershell
rtk docker compose up -d
rtk dotnet ef database update --project src/OffersService
rtk dotnet run --project src/OffersService
rtk dotnet test
```

For PowerShell built-ins, wrap them:

```powershell
rtk powershell -NoProfile -Command "Get-ChildItem -Force"
```

## Implementation Expectations

- Prefer existing project patterns before introducing new abstractions.
- Keep EF Core queries intentional; avoid N+1 behavior.
- Use reversible migrations for schema changes.
- Every EF Core entity configuration must explicitly set the table name with `ToTable(...)` using constants from `TableNames`.
- Keep FluentValidation error messages centralized in focused message catalog classes under `src/OffersService/Validators`; do not duplicate literal validator messages inline in validators or tests.
- Keep feature-specific service registrations and middleware setup behind focused extension methods in `src/OffersService/IOC`.

## Architecture Notes

- Retailers are part of the seeded EF Core data model. `Offer.RetailerId` is nullable and uses `DeleteBehavior.SetNull`.
- `GET /api/offers` uses `OfferListRequestValidator` and returns a `PagedList<OfferListItemDto>` with `TotalCount`, page metadata, and navigation flags.
- `GET /api/retailers/{id}/stats` uses `RetailerStatsRequestValidator` and `IRetailerService` to return active-retailer aggregate stats in one EF Core query.

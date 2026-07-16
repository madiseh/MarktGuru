# AGENTS.md

## Core Rules

- Follow `C:\Users\madis\.codex\RTK.md`: run shell commands through `rtk`.
- Keep `AGENTS.md` and relevant files under `docs/` in sync with every future code, API, architecture, workflow, or test change.
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

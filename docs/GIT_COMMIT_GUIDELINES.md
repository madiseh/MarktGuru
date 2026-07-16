# Git Commit And Message Guidelines

Adapted from Skills Directory entries checked on 2026-07-16:

- Git Commit Helper: https://www.skillsdirectory.com/skills/alirezarezvani-git-commit-helper
- Crafting Commits: https://skillsdirectory.com/skills/shrwnsan-crafting-commits
- Conventional Commits: https://www.skillsdirectory.com/skills/vm0-ai-conventional-commits

## Goal

Generate clear Conventional Commit messages from the actual git diff, with enough context for future maintainers to understand why the change exists.

## Required Format

```text
<type>(<scope>): <subject>

<body>

<footer>
```

Scope, body, and footer are optional. Use them when they add useful context.

## Common Types

| Type | Use |
|------|-----|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation-only change |
| `test` | Test additions or updates |
| `refactor` | Code restructuring without feature or bug behavior |
| `perf` | Performance improvement |
| `chore` | Maintenance work |
| `build` | Build system or dependency changes |
| `ci` | CI configuration changes |

## Message Rules

- Inspect `git status` and the staged diff before drafting a message.
- Write the subject in imperative mood, for example `docs: add project agent instructions`.
- Keep the subject concise and do not end it with a period.
- Use the body to explain why the change was made when the subject is not enough.
- Include testing notes in the body for risky, broad, or behavior-changing work.
- Mention breaking changes in the footer with `BREAKING CHANGE:`.

## Workflow

1. Review the working tree.
2. Stage only files that belong to the same logical change.
3. Draft a Conventional Commit message from the staged diff.
4. Confirm the message matches the intent of the change.
5. Commit.

## Examples

```text
docs: add project agent instructions
```

```text
feat(offers): add paginated offer listing

Adds filtering by product, retailer, and status while keeping pagination totals
based on the unpaged result set.
```

```text
fix(import): reject invalid offer dates

Prevents imported offers from creating invalid validity windows and adds tests
for the rejected payloads.
```

## Boundaries

- Do not hide unrelated edits in one commit.
- Do not commit generated or local-only files unless they are intentionally part of the project.
- Do not rewrite existing history unless the user explicitly asks for it.

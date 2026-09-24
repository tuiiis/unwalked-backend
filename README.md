# Backend

ASP.NET Core (C#) + PostgreSQL/PostGIS. Places catalogue, recommendations, sync, achievement sharing.

## Layout

| Path | Purpose |
|---|---|
| `src/` | Solution: Api, Application, Domain, Infrastructure |
| `tests/` | Unit and integration tests — the Back-end criterion needs >= 70% coverage |
| `db/migrations/` | DDL and EF Core migrations, versioned in git (Database criterion) |
| `db/` | Data dictionary, ER diagram, seed scripts, roles and grants |
| `api/` | OpenAPI 3.0 spec, Getting Started, integration tutorial, changelog (API documentation criterion) |
| `docs/adr/` | Architecture decision records |
| `deploy/` | Dockerfile, compose, cloud deployment docs (Cloud criterion) |

## Rules

- Secrets never in the repo. Local: user-secrets / `.env` (gitignored). Cloud: platform secret store.
- Every schema change is a migration in git, no manual edits on the server.

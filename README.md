# Unwalked: back-end

Back-end for the bachelor thesis "Mobile Application for Recommending New Places and Walking Routes Based on the Analysis of the User's Habitual Geography". Client: [unwalked-android](https://github.com/tuiiis/unwalked-android).

ASP.NET Core 10, PostgreSQL 17 with PostGIS 3.5, EF Core. The API serves a place catalogue, accounts for achievement shares and anonymous feedback on recommendations. The catalogue starts with Vilnius from Overture Places, and areas elsewhere are loaded from OpenStreetMap when a client asks for them.

Public instance: https://unwalked-api-7820623db8c8.herokuapp.com, Swagger UI at [/swagger](https://unwalked-api-7820623db8c8.herokuapp.com/swagger). It runs on Heroku in the EU with the database on Neon in Frankfurt, as described in [Deployment](docs/deployment.md).

## Documentation

| Topic | Where |
|---|---|
| API: getting started, tutorials, guides, reference, changelog | [docs/api](docs/api/README.md) |
| OpenAPI specification | [docs/api/openapi.yaml](docs/api/openapi.yaml) |
| Database: schema, dictionary, roles, seeds | [docs/database/data-dictionary.md](docs/database/data-dictionary.md), DDL in [db/schema.sql](db/schema.sql) |
| Deployment | [docs/deployment.md](docs/deployment.md) |

## Project layout

```text
src/
  Unwalked.Domain/          entities, value objects, H3 and geo rules, no dependencies
  Unwalked.Application/     use cases and the interfaces they need
  Unwalked.Infrastructure/  EF Core and Npgsql, migrations, JWT, password hashing, OpenStreetMap loading
  Unwalked.Api/             controllers, contracts, error handling, OpenAPI
  Unwalked.Tools/           command line: migrate, seed, load-places
tests/
  Unwalked.UnitTests/       NUnit, Domain, Application and Infrastructure parts without a database
  Unwalked.IntegrationTests/ the whole API against PostGIS in Testcontainers
db/                         logins, reference and test seeds, generated schema.sql
docs/                       API, database and deployment documentation
```

Each layer depends only on the ones above it in this list.

## Running locally

Requires Docker. Copy the environment file and set your own passwords and a signing key of at least 32 characters.

```bash
cp .env.example .env
docker compose up --build
```

This starts PostgreSQL, applies the migrations, loads reference data and runs the API on http://localhost:18080 with Swagger UI at `/swagger`. The database listens on `localhost:55432`.

Add test accounts, achievement cards, reactions and region packs.

```bash
docker compose --profile test-data run --rm seed-test
```

Test accounts are `ona@example.test`, `jonas@example.test` and `rasa@example.test` with the password `Unwalked-test-1`.

The Vilnius catalogue is not stored in the repository. Export it from the public Overture Maps release 2026-09-23.1 (CDLA-Permissive-2.0) with [DuckDB](https://duckdb.org/docs/installation/), then load it.

```bash
mkdir -p ../data/out
(cd ../data/out && duckdb < "$OLDPWD/tools/export-vilnius-places.sql")
docker compose --profile load run --rm load
```

## Database logins

| Login | Purpose |
|---|---|
| `unwalked_migrator` | Migrations, seeds and the catalogue loader |
| `unwalked_api` | The API: reads, plus inserts for accounts, shares, feedback and loaded areas. No updates or deletes on the catalogue |
| `unwalked_analyst` | Research queries on feedback and masked accounts only |

Passwords come from `.env`, which is never committed.

## Tests

```bash
dotnet test
```

Requires the .NET SDK from `global.json` and Docker for Testcontainers. 153 unit tests and 57 integration tests. The integration tests apply the migrations to a real PostGIS and check the API together with constraints, the trigger, the procedure, the views and role permissions.

Coverage on 28.11.2026 is 98.8% of lines and 94.6% of branches. Unit tests alone cover the business logic: Domain 95.9%, Application 95.3%. To build the report:

```bash
dotnet test --settings coverage.runsettings --collect "XPlat Code Coverage" --results-directory TestResults
dotnet tool restore
dotnet reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/report -reporttypes:Html
```

## Continuous integration

[ci.yml](.github/workflows/ci.yml) runs on every push and pull request. Heroku deploys a commit only after all four jobs pass.

| Job | What it checks |
|---|---|
| Tests and coverage | Build with the .NET code analysers at `latest-recommended`, any warning fails it. Unit and integration tests, fails below 80% line coverage |
| Vulnerable packages | Any NuGet package with a known vulnerability, including transitive ones |
| OpenAPI lint | The specification with Spectral and Redocly |
| Docker image | The production image builds. On `main` it is pushed to `ghcr.io/tuiiis/unwalked-backend` |

## Where the Back-end criterion is implemented

| Requirement | Where |
|---|---|
| Framework, ORM, database | ASP.NET Core 10, EF Core with Npgsql and NetTopologySuite, PostgreSQL with PostGIS |
| Layered architecture | The four projects in [Project layout](#project-layout) |
| API description | Swagger UI, [openapi.yaml](docs/api/openapi.yaml), XML comments on the public API |
| Global error handling | `Api/ErrorHandling/ApplicationExceptionHandler.cs`, Problem Details with a `traceId` |
| Structured logging | Serilog with compact JSON in `Api/Program.cs` |
| Authentication and passwords | JWT bearer tokens in `Infrastructure/Security`, PBKDF2 hashing through ASP.NET Identity |
| Validation and status codes | Domain rules raise validation errors, mapped to 400, 401, 404, 409 and 429 |
| Account deletion | `DELETE /api/v1/account` removes the account and its shares, the server half of GDPR erasure |
| Caching | Output cache on the GET place searches, dropped when an area finishes loading |
| Secrets | Environment variables and Heroku config vars only, `.env` is git-ignored |
| Production deployment | Heroku and Neon, [docs/deployment.md](docs/deployment.md) |
| Tests and coverage | [Tests](#tests) |
| CI/CD with quality gates | [Continuous integration](#continuous-integration) |

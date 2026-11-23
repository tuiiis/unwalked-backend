# Unwalked: back-end

Back-end for the bachelor thesis "Mobile Application for Recommending New Places and Walking Routes Based on the Analysis of the User's Habitual Geography". Documentation and business analysis: [unwalked-thesis](https://github.com/tuiiis/unwalked-thesis).

ASP.NET Core 10 with PostgreSQL 17 and PostGIS 3.5. Hosting: a Docker web service on Render and managed PostgreSQL on Neon, both in the EU (Frankfurt). A public instance runs at https://unwalked-api.onrender.com with Swagger UI at https://unwalked-api.onrender.com/swagger. [Deployment](docs/deployment.md) explains how to set them up.

## API documentation

Start at [docs/api](docs/api/README.md). It has a Getting started guide, two step-by-step tutorials, guides on authentication, errors, limits, idempotency and versioning, and the [OpenAPI specification](docs/api/openapi.yaml). A running instance serves Swagger UI at http://localhost:18080/swagger.

## Database

| Document | Content |
|---|---|
| [Data dictionary](docs/database/data-dictionary.md) | Logical schema, every table and column, indexes, views, routines, roles, personal data, and the on-device Room schema |
| [db/schema.sql](db/schema.sql) | Full DDL generated from the migrations |
| `src/Unwalked.Infrastructure/Persistence/Migrations` | EF Core migrations, the source of truth |
| `db/init` | Logins and group roles created when the container starts |
| `db/seed/reference` | Reference data for every environment |
| `db/seed/test` | Test data for local development |

## Running the database only

Requires Docker. Copy the environment file and set your own passwords.

```bash
cp .env.example .env
```

Start PostgreSQL, apply the migrations and load reference data. The API is not started.

```bash
docker compose run --rm --build seed
```

Add test data.

```bash
docker compose --profile test-data run --rm seed-test
```

Connect as any of the [database logins](#database-logins) with its password from `.env`.

```bash
docker compose exec db psql -h localhost -U unwalked_analyst -d unwalked
```

The place catalogue is exported and loaded as described below. Remove the database with `docker compose down -v`.

## Running locally

Requires Docker. Copy the environment file and set your own passwords and a signing key of at least 32 characters.

```bash
cp .env.example .env
```

Start the database, apply migrations, load reference data and run the API.

```bash
docker compose up --build
```

The API listens on http://localhost:18080 and the database on `localhost:55432`.

Add test accounts, achievement cards, reactions and region packs.

```bash
docker compose --profile test-data run --rm seed-test
```

The place catalogue is not stored in the repository. It is exported from the public Overture Maps release 2026-09-23.1 (CDLA-Permissive-2.0) for the Vilnius region bounds. The export needs [DuckDB](https://duckdb.org/docs/installation/) and network access.

```bash
mkdir -p ../data/out
(cd ../data/out && duckdb < "$OLDPWD/tools/export-vilnius-places.sql")
```

Then load it. `PLACES_DATA_DIR` points to `../data/out` by default.

```bash
docker compose --profile load run --rm load
```

Test accounts are `ona@example.test`, `jonas@example.test` and `rasa@example.test` with the password `Unwalked-test-1`.

## Database logins

| Login | Purpose |
|---|---|
| `unwalked_migrator` | Migrations, seeds and the catalogue loader |
| `unwalked_api` | The API, read access plus the few writes it needs |
| `unwalked_analyst` | Research queries on reactions and masked accounts only |

Passwords come from `.env`, which is never committed.

## Tools

```bash
dotnet run --project src/Unwalked.Tools -- migrate
dotnet run --project src/Unwalked.Tools -- seed db/seed/reference
dotnet run --project src/Unwalked.Tools -- load-places vilnius path/to/vilnius_places.csv
```

The tools read the connection string from `ConnectionStrings__Unwalked`.

## Tests

Unit tests use NUnit. Integration tests start PostGIS in Testcontainers, apply the migrations and the reference seed, and check the API together with database constraints, the trigger, the procedure, the views and role permissions.

Requires the .NET SDK version from `global.json` (10.0.100 or a newer feature band) and Docker for Testcontainers.

```bash
dotnet test
```

Coverage is collected with coverlet and turned into an HTML report with ReportGenerator, both pinned in the repository.

```bash
dotnet test --settings coverage.runsettings --collect "XPlat Code Coverage" --results-directory TestResults
dotnet tool restore
dotnet reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:TestResults/report -reporttypes:"Html;TextSummary"
```

Open `TestResults/report/index.html`. On 22.11.2026 the suite had 181 tests with 98.2% line and 93% branch coverage.

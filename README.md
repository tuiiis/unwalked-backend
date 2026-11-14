# Unwalked: back-end

Back-end for the bachelor thesis "Mobile Application for Recommending New Places and Walking Routes Based on the Analysis of the User's Habitual Geography". Documentation and business analysis: [unwalked-thesis](https://github.com/tuiiis/unwalked-thesis).

ASP.NET Core 10 with PostgreSQL 17 and PostGIS 3.5. Hosting: a Docker web service on Render and managed PostgreSQL on Neon, both in the EU (Frankfurt).

## Database

| Document | Content |
|---|---|
| [Data dictionary](docs/database/data-dictionary.md) | Logical schema, every table and column, indexes, views, routines, roles, personal data, and the on-device Room schema |
| [db/schema.sql](db/schema.sql) | Full DDL generated from the migrations |
| `src/Unwalked.Infrastructure/Persistence/Migrations` | EF Core migrations, the source of truth |
| `db/init` | Logins and group roles created when the container starts |
| `db/seed/reference` | Reference data for every environment |
| `db/seed/test` | Test data for local development |

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

Load the Vilnius place catalogue from an Overture CSV. The folder is set by `PLACES_DATA_DIR`.

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

```bash
dotnet test
```

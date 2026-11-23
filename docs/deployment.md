# Deployment

The production API runs as a Docker web service on Render. The database is managed PostgreSQL with PostGIS on Neon. Both are in Frankfurt and both run on free plans.

| Part | Service | Plan limits that matter |
|---|---|---|
| API | Render web service built from the `Dockerfile` | 512 MB RAM, sleeps after 15 minutes without traffic, the first request after that takes about a minute |
| Database | Neon PostgreSQL 17 with PostGIS | 1 GB storage, compute suspends after 5 minutes idle and wakes on the next connection |

Render deploys every push to `main` automatically using [render.yaml](../render.yaml). Secrets live only in the Render and Neon dashboards and in a local `.env.neon` file that git ignores.

## First deployment

### 1. Database

Create a Neon project in AWS Europe Central (Frankfurt) with PostgreSQL 17, then add a database named `unwalked` owned by the default owner role. Copy the host name of the direct connection, not the pooled one.

Export the place catalogue to `../data/out/vilnius_places.csv` as described in the [README](../README.md#running-locally). Then run the setup script with the Neon host.

```bash
tools/deploy-neon.sh ep-example-123456.eu-central-1.aws.neon.tech
```

The script asks for the owner password without echoing it and then:

1. Creates the group roles and the three logins from `db/init/01-roles.sh` with random passwords, stored in `.env.neon`
2. Applies the migrations, loads the reference data and loads the place catalogue as `unwalked_migrator`
3. Writes the connection string for the API login to `.env.neon` as `API_CONNECTION_STRING`

Set `TEST_DATA=1` to also load the test accounts. Set `SKIP_ROLES=1` to rerun it after the roles already exist.

### 2. API

In Render choose New, then Blueprint, and connect this repository. Render reads `render.yaml`, generates the JWT signing key and asks for `ConnectionStrings__Unwalked`. Paste the value of `API_CONNECTION_STRING` from `.env.neon`.

When the deploy finishes, check the service.

```bash
curl https://unwalked-api.onrender.com/health
```

```text
Healthy
```

## Later deployments

Push to `main`. Render builds the image and switches traffic once `/health` answers. Schema changes run from a laptop before the push.

```bash
SKIP_ROLES=1 tools/deploy-neon.sh ep-example-123456.eu-central-1.aws.neon.tech
```

## Logs

Render shows the API output in the service Logs tab. Every line is a compact JSON event from Serilog, so it can be filtered by level, request path or trace id.

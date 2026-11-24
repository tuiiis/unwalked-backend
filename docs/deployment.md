# Deployment

The production API runs as a Docker container on Heroku in the EU region. The database is managed PostgreSQL with PostGIS on Neon in Frankfurt.

| Part | Service | Plan |
|---|---|---|
| API | Heroku Basic web dyno built from the `Dockerfile` through [heroku.yml](../heroku.yml) | Always on, 512 MB RAM. Paid from the GitHub Student Developer Pack credit |
| Database | Neon PostgreSQL 17 with PostGIS | Free plan with 1 GB storage. Compute suspends after 5 minutes idle and wakes on the next connection |

The database stays on Neon because the API connects with its own restricted login. Neon allows the three logins from `db/init/01-roles.sh`, while the Heroku Postgres plans in this price range give a single owner account only.

Every push to `main` runs the [CI workflow](../.github/workflows/ci.yml). Heroku is connected to the GitHub repository and deploys the commit only after all CI checks pass. Secrets live only in Heroku config vars, in the Neon console and in a local `.env.neon` file that git ignores.

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

Requires the [Heroku CLI](https://devcenter.heroku.com/articles/heroku-cli). Create the app in the EU region on the container stack.

```bash
heroku create unwalked-api --region eu --stack container
```

Set the connection string and a fresh signing key. Neither value is printed.

```bash
heroku config:set -a unwalked-api ConnectionStrings__Unwalked="$(. ./.env.neon && echo "$API_CONNECTION_STRING")" Jwt__SigningKey="$(openssl rand -base64 48)"
```

In the Heroku dashboard open the app, then Deploy, and connect the GitHub repository. Enable automatic deploys from `main` with "Wait for GitHub checks to pass before deploy" ticked. Start the first build with Deploy Branch, then switch the dyno to Basic.

```bash
heroku ps:type web=basic -a unwalked-api
```

Check the service.

```bash
curl https://unwalked-api-7820623db8c8.herokuapp.com/health
```

```text
Healthy
```

## Later deployments

Push to `main`. GitHub Actions builds and tests the commit. When every check passes, Heroku builds the image from `heroku.yml` and releases it. A failed check leaves the running version in place. Schema changes run from a laptop before the push.

```bash
SKIP_ROLES=1 tools/deploy-neon.sh ep-example-123456.eu-central-1.aws.neon.tech
```

## Logs

Every line the API writes is a compact JSON event from Serilog, so it can be filtered by level, request path or trace id.

```bash
heroku logs -a unwalked-api --tail
```

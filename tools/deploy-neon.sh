#!/bin/sh
set -eu
cd "$(dirname "$0")/.."

host=${1:?usage: tools/deploy-neon.sh <neon-host> [owner-role]}
owner=${2:-neondb_owner}
database=unwalked
secrets=.env.neon
places=${PLACES_DATA_DIR:-../data/out}
image=unwalked-tools:deploy
ssl=${SSL_MODE:-Require}

if [ ! -f "$secrets" ]; then
  (
    umask 077
    {
      echo "MIGRATOR_PASSWORD=$(openssl rand -hex 24)"
      echo "API_PASSWORD=$(openssl rand -hex 24)"
      echo "ANALYST_PASSWORD=$(openssl rand -hex 24)"
    } > "$secrets"
  )
fi
set -a
. "./$secrets"
set +a

printf 'Password for %s: ' "$owner"
stty -echo 2>/dev/null || true
read -r PGPASSWORD
stty echo 2>/dev/null || true
echo

if [ "${SKIP_ROLES:-0}" != 1 ]; then
  export PGHOST="$host" PGPASSWORD PGSSLMODE="$(echo "$ssl" | tr A-Z a-z)" POSTGRES_USER="$owner" POSTGRES_DB="$database"
  docker run --rm -i \
    -e PGHOST -e PGPASSWORD -e PGSSLMODE -e POSTGRES_USER -e POSTGRES_DB \
    -e MIGRATOR_PASSWORD -e API_PASSWORD -e ANALYST_PASSWORD \
    "${POSTGIS_IMAGE:-imresamu/postgis:17-3.5}" sh -s < db/init/01-roles.sh
fi

docker build --target tools -t "$image" .

export ConnectionStrings__Unwalked="Host=$host;Database=$database;Username=unwalked_migrator;Password=$MIGRATOR_PASSWORD;SSL Mode=$ssl;Gss Encryption Mode=Disable;Command Timeout=600"
tools() {
  docker run --rm -e ConnectionStrings__Unwalked -v "$PWD/db/seed:/seed:ro" -v "$(cd "$places" && pwd):/data:ro" "$image" "$@"
}

tools migrate
tools seed /seed/reference
if [ "${TEST_DATA:-0}" = 1 ]; then
  tools seed /seed/test
fi
tools load-places vilnius /data/vilnius_places.csv

api="Host=$host;Database=$database;Username=unwalked_api;Password=$API_PASSWORD;SSL Mode=$ssl;Gss Encryption Mode=Disable;Maximum Pool Size=10"
grep -v '^API_CONNECTION_STRING=' "$secrets" > "$secrets.tmp" || true
echo "API_CONNECTION_STRING='$api'" >> "$secrets.tmp"
mv "$secrets.tmp" "$secrets"
chmod 600 "$secrets"

echo "Done. The connection string for the API is in $secrets under API_CONNECTION_STRING."

#!/bin/sh
set -eu

psql -v ON_ERROR_STOP=1 \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set migrator_password="$MIGRATOR_PASSWORD" \
  --set api_password="$API_PASSWORD" \
  --set analyst_password="$ANALYST_PASSWORD" <<'SQL'
CREATE EXTENSION IF NOT EXISTS postgis;

CREATE ROLE app_read NOLOGIN;
CREATE ROLE app_write NOLOGIN IN ROLE app_read;
CREATE ROLE app_analyst NOLOGIN;
CREATE ROLE admin NOLOGIN;

CREATE ROLE unwalked_migrator LOGIN PASSWORD :'migrator_password' IN ROLE admin;
CREATE ROLE unwalked_api LOGIN PASSWORD :'api_password' IN ROLE app_write;
CREATE ROLE unwalked_analyst LOGIN PASSWORD :'analyst_password' IN ROLE app_analyst;

REVOKE ALL ON DATABASE :"DBNAME" FROM PUBLIC;
GRANT CONNECT ON DATABASE :"DBNAME" TO app_read, app_analyst, admin;
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
GRANT USAGE, CREATE ON SCHEMA public TO admin;
GRANT USAGE ON SCHEMA public TO app_read, app_analyst;
SQL

#!/bin/sh
set -eu
migrations="$(dirname "$0")/../src/Unwalked.Infrastructure/Persistence/Migrations"
perl -i -ne 's/^\x{EF}\x{BB}\x{BF}//; print unless m{^\s*//}' "$migrations"/*.cs

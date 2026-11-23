# Versioning

## Scheme

The major version is part of the path: `/api/v1/...`. The specification itself follows semantic versioning in `info.version`, currently `1.0.0`.

| Change | Example | Version |
|---|---|---|
| Breaking | Removing a field, renaming a parameter, making an optional parameter required, changing a status code | New path, `/api/v2` |
| Additive | A new endpoint, a new optional parameter, a new response field | Minor, `1.1.0` |
| Fix | A clearer description, a corrected example | Patch, `1.0.1` |

Clients must ignore response fields they do not know. That is what makes additive changes safe.

## Deprecation

When `/api/v2` appears, `/api/v1` keeps working for at least three months. Deprecated operations are marked `deprecated: true` in the specification and answer with a `Deprecation` header and a `Sunset` header carrying the removal date.

## Where changes are recorded

Every change to the API lands in the [changelog](../CHANGELOG.md) together with the version it belongs to. The committed [openapi.yaml](../openapi.yaml) is regenerated in the same commit as the code change, an integration test fails otherwise.

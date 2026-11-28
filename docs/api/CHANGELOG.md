# Changelog

All notable changes to the API are listed here. The format follows Keep a Changelog and versions follow semantic versioning.

## [1.2.0] - 2026-11-28

### Added

- Account: `DELETE /account` deletes the caller's account and every share it published
- `Age` header on cached place searches

### Changed

- `GET /places/near` and `GET /places/in-bbox` are cached for 10 minutes, and the cache is dropped when an area finishes loading
- Publishing a share with a token of a deleted account answers `401` instead of failing on the database

## [1.1.0] - 2026-11-24

### Added

- Areas: `GET /areas/{cell}` and `PUT /areas/{cell}`. An area outside the preloaded regions is filled with named places from OpenStreetMap in the background, so the catalogue works beyond Vilnius
- `429 Too Many Requests` with a `Retry-After` header when area loading limits are reached

## [1.0.0] - 2026-11-22

### Added

- Places: `GET /places/near`, `GET /places/in-bbox`, `POST /places/by-cells` over the Overture Places catalogue for Vilnius
- Accounts: `POST /auth/register`, `POST /auth/login` with JWT bearer tokens valid for 30 days
- Feedback: `POST /feedback`, idempotent by `eventId`, and `GET /feedback/stats` per recommendation type
- Shares: create, read, image and delete for public achievement links
- Problem Details error format with `traceId` on every error
- Request and response examples for every operation
- OpenAPI document served as YAML and JSON under `/openapi/v1`

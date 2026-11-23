# Architecture

## System context

The phone does the heavy lifting. It records location, detects stays, aggregates them into H3 cells and computes novelty. The API is a thin catalogue and sharing service. It never receives location history.

```mermaid
flowchart LR
    user([Walker])
    friend([Friend with a link])
    app["Unwalked Android app<br/>history, cells, recommendations<br/>stay on the device"]
    api["Unwalked API<br/>ASP.NET Core 10"]
    db[("PostgreSQL 17 + PostGIS<br/>catalogue, shares, feedback")]
    overture[(Overture Places<br/>monthly release)]
    tiles[(Protomaps tiles)]

    user --> app
    app -- "places by cells or bbox<br/>anonymous feedback<br/>shares with token" --> api
    friend -- "open share link" --> api
    api --> db
    overture -. "export and load, offline" .-> db
    app -- "map tiles" --> tiles
```

## Resources

| Resource | Purpose | Auth | Stores personal data |
|---|---|---|---|
| `auth` | Accounts for share owners | None | Email and password hash |
| `places` | Read-only catalogue from Overture Places | None | No |
| `feedback` | Anonymous reactions to recommendations, for the thesis study | None | No, no account id or location |
| `shares` | Public achievement links with a generalised image | Token to create and delete | Owner id, never exposed |

## Inside the API

The code follows a layered layout. Each layer only depends on the ones below it.

```mermaid
flowchart TB
    api["Unwalked.Api<br/>controllers, contracts, error handler, OpenAPI"]
    app["Unwalked.Application<br/>services: PlaceQuery, Auth, Feedback, Share"]
    domain["Unwalked.Domain<br/>value objects and rules: GeoCoordinate, H3Cell, PasswordPolicy"]
    infra["Unwalked.Infrastructure<br/>EF Core, Npgsql, PostGIS queries, JWT, password hashing"]
    api --> app --> domain
    infra --> app
    api -. "composition root" .-> infra
```

Validation lives in the domain. A `GeoCoordinate` cannot be constructed with latitude 95, so every endpoint that takes a point rejects it the same way with the same message.

## Request lifecycle

What happens to `GET /api/v1/places/near?lat=54.6872&lon=25.2797&radius=300`.

```mermaid
sequenceDiagram
    autonumber
    participant App as Android app
    participant MW as Middleware
    participant C as PlacesController
    participant S as PlaceQueryService
    participant D as Domain
    participant R as PlaceReadRepository
    participant DB as PostgreSQL + PostGIS

    App->>MW: GET /api/v1/places/near
    MW->>MW: forwarded headers, request logging, auth (skipped, anonymous)
    MW->>C: bind lat, lon, radius, category, limit
    C->>S: FindNearAsync
    S->>D: GeoCoordinate, SearchRadius, ResultLimit
    alt invalid value
        D-->>MW: DomainValidationException
        MW-->>App: 400 problem+json with errors and traceId
    else valid
        S->>R: query
        R->>DB: ST_DWithin on geography, ORDER BY distance
        DB-->>R: rows
        R-->>S: PlaceSummary list
        S-->>C: places
        C-->>App: 200 JSON array
    end
```

## Sharing flow

```mermaid
sequenceDiagram
    autonumber
    participant App as Android app
    participant API as Unwalked API
    participant DB as PostgreSQL
    participant F as Friend

    App->>API: POST /auth/login
    API->>DB: find account, verify hash
    API-->>App: 200 accessToken (30 days)
    App->>App: render card, blur home area
    App->>API: POST /shares + Bearer token + image
    API->>API: check token, magic bytes, size
    API->>DB: insert share with owner id
    API-->>App: 201 Location: /shares/{id}
    App->>F: Android share sheet with link
    F->>API: GET /shares/{id}
    API-->>F: name, date, imageUrl (no owner)
    F->>API: GET /shares/{id}/image
    API-->>F: image bytes
```

## Database logins

The API connects as `unwalked_api`, which can read the catalogue and write only feedback, accounts and shares. It cannot change the schema or the catalogue. The [data dictionary](../database/data-dictionary.md) has the full role matrix.

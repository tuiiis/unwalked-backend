# API reference

The full reference is the [OpenAPI specification](../openapi.yaml). This page is a map of it. For an interactive version start the API and open http://localhost:18080/swagger, or render a static page with Redoc.

```bash
npx @redocly/cli@2.11.1 build-docs docs/api/openapi.yaml -o redoc.html
```

Base path: `/api/v1`. All bodies are JSON in UTF-8. Errors are `application/problem+json`, refer to [Errors](../guides/errors.md).

## Auth

| Method | Path | Operation | Success | Errors |
|---|---|---|---|---|
| POST | `/auth/register` | `register` | 201 `RegisterResponse` | 400, 409 |
| POST | `/auth/login` | `login` | 200 `LoginResponse` | 400, 401 |

## Places

| Method | Path | Operation | Success | Errors |
|---|---|---|---|---|
| GET | `/places/near` | `findPlacesNear` | 200 `PlaceResponse[]` | 400 |
| GET | `/places/in-bbox` | `findPlacesInBoundingBox` | 200 `PlaceResponse[]` | 400 |
| POST | `/places/by-cells` | `findPlacesByCells` | 200 `PlaceResponse[]` | 400 |

## Feedback

| Method | Path | Operation | Success | Errors |
|---|---|---|---|---|
| POST | `/feedback` | `submitFeedback` | 201 new, 200 repeat | 400 |
| GET | `/feedback/stats` | `getFeedbackStats` | 200 `FeedbackStatsResponse` | 400 |

## Shares

| Method | Path | Operation | Auth | Success | Errors |
|---|---|---|---|---|---|
| POST | `/shares` | `createShare` | Bearer | 201 `ShareResponse` + `Location` | 400, 401 |
| GET | `/shares/{id}` | `getShare` | None | 200 `ShareResponse` | 404 |
| GET | `/shares/{id}/image` | `getShareImage` | None | 200 image bytes | 404 |
| DELETE | `/shares/{id}` | `deleteShare` | Bearer | 204 | 401, 404 |

Every operation can also answer 500.

## Data models

| Schema | Fields |
|---|---|
| `PlaceResponse` | `id`, `externalId` (Overture id), `name`, `category`, `confidence` (0 to 1), `lat`, `lon`, `h3Cell` (resolution 11), `distanceMeters` (only from `near`) |
| `PlacesByCellsRequest` | `cells` (1 to 500 H3 ids, resolution 7 to 11), `category`, `limit` |
| `RegisterRequest`, `LoginRequest` | `email`, `password` |
| `LoginResponse` | `accessToken`, `tokenType` (`Bearer`), `expiresAt` |
| `SubmitFeedbackRequest` | `eventId` (UUID), `recommendationType`, `action` (`accepted`, `dismissed`, `visited`), `category` |
| `FeedbackStatsResponse` | `from`, `to`, `types[]` with `accepted`, `dismissed`, `visited`, `acceptanceRate`, `visitRate` |
| `CreateShareRequest` | `achievementName` (1 to 80 characters), `achievedOn` (not in the future), `image.contentType`, `image.data` (base64, at most 300 KB) |
| `ShareResponse` | `id`, `achievementName`, `achievedOn`, `imageUrl`, `createdAt` |

Recommendation types: `nearby_new_place`, `along_habitual_route`, `unvisited_cell`, `walking_route`.

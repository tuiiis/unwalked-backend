# Limits and paging

The places endpoints answer map questions: what is near this point, what is in this view, what is in these cells. A map never needs page 2. It needs the most relevant places for the area on screen. So version 1 uses a result limit and an area limit instead of cursor paging.

## Result limit

| Parameter | Default | Range |
|---|---|---|
| `limit` | 50 | 1 to 200 |

Results are ordered so that the first ones matter most.

| Operation | Order |
|---|---|
| `GET /places/near` | Nearest first, by geodesic distance |
| `GET /places/in-bbox` | Highest Overture confidence first |
| `POST /places/by-cells` | Highest Overture confidence first |

If you get exactly `limit` results there may be more. Zoom in, shrink the radius or add a `category` filter rather than asking for more.

## Area limits

| Operation | Limit | Why |
|---|---|---|
| `near` | Radius 10 to 5000 metres | A walk rarely goes further than 5 km from where it starts |
| `in-bbox` | At most 0.25 square degrees, about 55 by 32 km around Vilnius | Covers a whole city view, rejects accidental world-wide boxes |
| `by-cells` | At most 500 cells, at most 20,000 resolution 11 cells after expansion | Keeps one request under one database round trip |

Cells of resolution 7 to 11 are accepted. Each one is expanded to its resolution 11 children, so one resolution 7 cell already counts as 2,401 cells and nine of them go over the limit.

## Rate limits

Version 1 has no rate limiting. The API serves a single mobile app during a thesis study. Rate limiting is planned before any public release and will answer `429 Too Many Requests` with a `Retry-After` header.

## Feedback statistics range

`GET /feedback/stats` accepts a range of at most 366 days. Without parameters it covers the last 30 days up to today in UTC.

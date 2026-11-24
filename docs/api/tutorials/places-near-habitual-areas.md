# Tutorial: find places near habitual areas

The app knows where a person usually walks because it aggregates location history into H3 cells on the phone. To suggest something new it asks the server for places in cells next to the habitual ones, but never sends the history itself. This tutorial walks through that exchange.

You need the API running with the Vilnius catalogue loaded, as described in [Getting started](../getting-started.md).

Outside Vilnius, make sure the catalogue covers the area first. An area is the H3 cell of resolution 5 around the user, here Kaunas.

```bash
curl -i -X PUT http://localhost:18080/api/v1/areas/851f40d3fffffff
```

The first answer is `202` with `"status":"Loading"`. Ask again with `GET` every few seconds until the status is `Ready`, which usually takes under a minute.

## Step 1. Look around one point

Start with the simplest question: what is within 300 metres of here.

```bash
curl "http://localhost:18080/api/v1/places/near?lat=54.6872&lon=25.2797&radius=300&limit=2"
```

Each place carries `h3Cell`, the resolution 11 cell it lies in, and `distanceMeters`, the geodesic distance from the point. Results are nearest first.

## Step 2. Fill the map view

When the user pans the map, ask for places inside the visible box. Filter by category to keep the map readable.

```bash
curl "http://localhost:18080/api/v1/places/in-bbox?minLat=54.68&minLon=25.27&maxLat=54.69&maxLon=25.29&category=cafe&limit=2"
```

```json
[
  {"id":8382,"externalId":"9c022ea4-de07-496f-86bc-222ef8873252","name":"Cat Cafe","category":"cafe","confidence":0.997,"lat":54.68656777,"lon":25.28320642,"h3Cell":"8b1f4030c6e0fff"},
  {"id":7794,"externalId":"80a2b819-d5b8-4745-afae-b59d2232ac52","name":"Vero Cafe","category":"cafe","confidence":0.989,"lat":54.68815471,"lon":25.27488586,"h3Cell":"8b1f4030c786fff"}
]
```

A box larger than 0.25 square degrees answers 400 on `bbox`. Zoomed out that far the app shows the heat map only.

## Step 3. Ask for places in candidate cells

On the phone the app takes the habitual cells, finds neighbouring cells that were never visited and sends only those.

```bash
curl -X POST http://localhost:18080/api/v1/places/by-cells \
  -H "Content-Type: application/json" \
  -d '{"cells":["8b1f4030c610fff"],"limit":2}'
```

```json
[
  {"id":8247,"externalId":"a46a077e-cdc8-4751-af2b-f3c12b6564f1","name":"Katpėdėlė","category":"restaurant","confidence":0.963,"lat":54.68741925,"lon":25.27925319,"h3Cell":"8b1f4030c610fff"},
  {"id":8253,"externalId":"350917ff-dbcf-46dc-a0d4-1a9b41a1199a","name":"Triumph Lingerie Partner - UAB MEKTA, SC Akropolis","category":"lingerie_store","confidence":0.850,"lat":54.687243,"lon":25.27966,"h3Cell":"8b1f4030c610fff"}
]
```

Coarser cells save bytes. A single resolution 9 cell covers 49 resolution 11 cells.

```bash
curl -X POST http://localhost:18080/api/v1/places/by-cells \
  -H "Content-Type: application/json" \
  -d '{"cells":["891f4030c63ffff"],"category":"restaurant","limit":20}'
```

## Step 4. Report what the user did

When the user reacts to a recommendation, and only if they agreed to help the study, the app sends anonymous feedback. Generate the `eventId` once per tap.

```bash
curl -i -X POST http://localhost:18080/api/v1/feedback \
  -H "Content-Type: application/json" \
  -d '{"eventId":"7d1f9a52-3c1e-4b8a-9a43-2f0c6b1e5d10","recommendationType":"nearby_new_place","action":"accepted","category":"cafe"}'
```

```text
HTTP/1.1 201 Created
```

Sending exactly the same request again answers `200 OK` and stores nothing. Refer to [Idempotency](../guides/idempotency.md).

## Step 5. Read the study results

```bash
curl "http://localhost:18080/api/v1/feedback/stats?from=2026-10-01&to=2026-11-22"
```

Each recommendation type gets counts and two rates. A rate is absent when there is nothing to divide by, for example no accepted recommendations yet.

## Privacy check

At no step did the app send a track, a timestamp of a visit or a home location. The server learns which cells someone looked at, the same thing a map tile server learns.

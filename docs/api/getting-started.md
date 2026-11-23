# Getting started

This guide takes you from a fresh clone to a first authenticated request. It takes about five minutes if Docker is already installed.

## 1. Start the API

Copy the environment file and replace every `change-me` with your own value. The signing key must be at least 32 characters.

```bash
cp .env.example .env
```

Start the database and the API. The first run builds the images, applies migrations and loads reference data.

```bash
docker compose up --build -d
```

Add the test accounts and sample feedback.

```bash
docker compose --profile test-data run --rm seed-test
```

Check that the API is up.

```bash
curl http://localhost:18080/health
```

```text
Healthy
```

The place catalogue is empty until you load it. The [README](../../README.md#running-locally) explains how to export and load the Vilnius extract. Everything below except the places requests works without it.

## 2. Open the interactive reference

Open http://localhost:18080/swagger in a browser. Every endpoint can be tried from there, including the ones that need a token (use the Authorize button).

## 3. Make an anonymous request

Places near the Akropolis shopping centre in Vilnius, within 300 metres.

```bash
curl "http://localhost:18080/api/v1/places/near?lat=54.6872&lon=25.2797&radius=300&limit=2"
```

```json
[
  {"id":8253,"externalId":"350917ff-dbcf-46dc-a0d4-1a9b41a1199a","name":"Triumph Lingerie Partner - UAB MEKTA, SC Akropolis","category":"lingerie_store","confidence":0.850,"lat":54.687243,"lon":25.27966,"h3Cell":"8b1f4030c610fff","distanceMeters":5.4},
  {"id":8290,"externalId":"fa01feb7-83e4-48bd-b898-a679deb51e4f","name":"Sixt","category":"car_rental_service","confidence":0.800,"lat":54.687156677246094,"lon":25.279651641845703,"h3Cell":"8b1f4030c613fff","distanceMeters":5.7}
]
```

## 4. Sign in

The seed data has three test accounts. All use the password `Unwalked-test-1`.

```bash
curl -X POST http://localhost:18080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"ona@example.test","password":"Unwalked-test-1"}'
```

```json
{"accessToken":"eyJhbGciOiJIUzI1NiIs...","tokenType":"Bearer","expiresAt":"2026-11-22T00:10:29+00:00"}
```

Keep the token in a shell variable for the next step.

```bash
TOKEN=$(curl -s -X POST http://localhost:18080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"ona@example.test","password":"Unwalked-test-1"}' | jq -r .accessToken)
```

## 5. Make an authenticated request

Deleting a share that does not exist proves the token works. A valid token gets 404, a missing token gets 401.

```bash
curl -i -X DELETE http://localhost:18080/api/v1/shares/00000000-0000-0000-0000-000000000001 \
  -H "Authorization: Bearer $TOKEN"
```

```text
HTTP/1.1 404 Not Found
```

## Next steps

1. Follow [Tutorial: share an achievement](tutorials/share-an-achievement.md) for a complete flow with a real image
2. Read [Errors](guides/errors.md) before writing a client, every failure uses the same format

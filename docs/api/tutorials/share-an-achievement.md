# Tutorial: share an achievement

In this tutorial you build the flow the app uses when a person shares an achievement such as "10 new streets in November". You register, sign in, upload a generalised image, open the public link like a friend would, and finally delete the share.

You need the API running locally as described in [Getting started](../getting-started.md), plus `curl`, `jq` and `base64`. Every command below was run against a local instance and the outputs are real.

## Step 1. Register an account

Pick an email that is not in the seed data.

```bash
curl -i -X POST http://localhost:18080/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"walker@example.test","password":"long enough password"}'
```

```text
HTTP/1.1 201 Created
```

```json
{"userId":"0c3f6e1a-...-..."}
```

Run it again and you get `409 Conflict` with `"detail": "An account with this email already exists."`. A client should then offer to sign in.

## Step 2. Sign in and keep the token

```bash
TOKEN=$(curl -s -X POST http://localhost:18080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"walker@example.test","password":"long enough password"}' | jq -r .accessToken)
echo "${TOKEN:0:20}..."
```

The token is valid for 30 days. Refer to [Authentication](../guides/authentication.md) for where to store it on a device.

## Step 3. Prepare the image

The app renders the achievement card, blurs the area around home and exports it as PNG, JPEG or WebP of at most 300 KB. For this tutorial any small PNG will do.

```bash
IMAGE=$(base64 < card.png | tr -d '\n')
```

The server checks the first bytes of the file against the declared content type. Declaring `image/png` for a JPEG answers 400 on `image.contentType`.

## Step 4. Create the share

```bash
curl -i -X POST http://localhost:18080/api/v1/shares \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"achievementName\":\"10 new streets in November\",\"achievedOn\":\"2026-11-21\",\"image\":{\"contentType\":\"image/png\",\"data\":\"$IMAGE\"}}"
```

```text
HTTP/1.1 201 Created
Location: http://localhost:18080/api/v1/shares/185affbe-18ad-4510-a624-8945c4c100ff
```

```json
{
  "id": "185affbe-18ad-4510-a624-8945c4c100ff",
  "achievementName": "10 new streets in November",
  "achievedOn": "2026-11-21",
  "imageUrl": "/api/v1/shares/185affbe-18ad-4510-a624-8945c4c100ff/image",
  "createdAt": "2026-11-21T23:10:29.5148956+00:00"
}
```

Save the `id`. The `Location` header is the public link the app puts into the Android share sheet.

```bash
SHARE_ID=185affbe-18ad-4510-a624-8945c4c100ff
```

## Step 5. Open the link as a friend

A friend has no account and no token. Both public requests work without one.

```bash
curl http://localhost:18080/api/v1/shares/$SHARE_ID
curl -o shared.png http://localhost:18080/api/v1/shares/$SHARE_ID/image
```

The response never contains the owner. Only the name, date and image are public.

## Step 6. Delete the share

```bash
curl -i -X DELETE http://localhost:18080/api/v1/shares/$SHARE_ID \
  -H "Authorization: Bearer $TOKEN"
```

```text
HTTP/1.1 204 No Content
```

Opening the link now answers 404. Deleting it again also answers 404, which the app treats as already done.

## What can go wrong

| Symptom | Cause | Fix |
|---|---|---|
| 401 on step 4 | Token missing or older than 30 days | Repeat step 2 |
| 400 on `image.data` | Not base64, or larger than 300 KB | Re-encode, compress the image |
| 400 on `achievedOn` | A date in the future | Use today or earlier |
| 404 on step 6 | The share was created by another account | Sign in as the owner |

## Next

[Tutorial: find places near habitual areas](places-near-habitual-areas.md) shows the read side of the API.

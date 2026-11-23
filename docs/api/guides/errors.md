# Errors

Every error uses the Problem Details format from RFC 9457 with the content type `application/problem+json`. A client needs one error parser for the whole API.

## Structure

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "lat": ["Latitude must be between -90 and 90."]
  },
  "traceId": "0HNQ6B3DSL1EP:00000001"
}
```

| Field | Always present | Meaning |
|---|---|---|
| `type` | Yes | A link to the HTTP status definition |
| `title` | Yes | A short, stable summary. Safe to match on |
| `status` | Yes | The HTTP status code, repeated for convenience |
| `detail` | Sometimes | A human-readable explanation of this particular failure |
| `errors` | Only on 400 | Each invalid field mapped to one or more messages |
| `traceId` | Yes | Identifies the request in the server logs. Quote it in bug reports |

Field names in `errors` match the request. Nested fields use a dot, for example `image.contentType`.

## Status codes

| Status | Title | When | What the client should do |
|---|---|---|---|
| 400 | One or more validation errors occurred. | A parameter or body field is missing, out of range or malformed | Show the message next to the field. Do not retry unchanged |
| 401 | Authentication failed. | Login with wrong credentials | Ask the user to check the email and password |
| 401 | Unauthorized | Missing, expired or invalid token | Sign in again, then retry once |
| 404 | Resource not found. | The share does not exist or is not yours | Treat as gone. Remove it from local state |
| 409 | Conflict. | Registering an email that already exists | Offer to sign in instead |
| 499 | Request cancelled. | The client closed the connection first | Nothing, the client already moved on |
| 500 | An unexpected error occurred. | A bug or an outage | Retry later with backoff. Report the `traceId` |

A 500 response never contains a stack trace or an exception message. The details are only in the server log, under the same `traceId`.

## Validation messages

These are the messages a client will meet most often.

| Field | Message |
|---|---|
| `lat` | Latitude must be between -90 and 90. |
| `lon` | Longitude must be between -180 and 180. |
| `radius` | Radius must be between 10 and 5000 metres. |
| `limit` | Limit must be between 1 and 200. |
| `category` | Category must be 1-64 characters of a-z, 0-9 or underscore. |
| `bbox` | The bounding box must not exceed 0.25 square degrees. |
| `cells` | 'abc' is not an H3 cell. |
| `email` | Email address is not valid. |
| `password` | Password must be 10-128 characters long. |
| `recommendationType` | 'teleport' is not one of: nearby_new_place, along_habitual_route, unvisited_cell, walking_route. |
| `image.contentType` | Image must be a PNG, JPEG or WebP file matching its declared content type. |
| `to` | The range must not exceed 366 days. |

The [specification](../openapi.yaml) has a full example body for every error each operation can return.

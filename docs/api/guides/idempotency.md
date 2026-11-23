# Idempotency

A phone on a walk loses signal all the time. The app must be able to resend feedback without counting it twice.

## Feedback

`POST /api/v1/feedback` is idempotent by `eventId`. The app generates a random UUID when the user taps a button and keeps it until the server confirms.

| Attempt | Response | Stored |
|---|---|---|
| First time this `eventId` is seen | `201 Created` | Yes |
| Any repeat of the same `eventId` | `200 OK` | Nothing new |

Both answers mean success. The client can delete the event from its outbox on either one.

The server compares only `eventId`. A repeat with a different action is still a repeat and is ignored, so generate a new id for every new tap.

## Other operations

| Operation | Safe to retry |
|---|---|
| All `GET` requests | Yes |
| `DELETE /shares/{id}` | Yes. A second delete answers 404, which the client should treat as done |
| `POST /auth/register` | Yes. A repeat answers 409, then sign in |
| `POST /auth/login` | Yes. Each call returns a new token |
| `POST /shares` | No. Each call creates a new link. Wait for the response before retrying |

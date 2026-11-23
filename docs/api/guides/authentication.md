# Authentication

Most of the API is anonymous. Only creating and deleting achievement shares needs an account, because a share must have an owner who can remove it later.

| Operation | Token needed |
|---|---|
| `POST /api/v1/auth/register`, `POST /api/v1/auth/login` | No |
| All `places` operations | No |
| `POST /api/v1/feedback`, `GET /api/v1/feedback/stats` | No, feedback is anonymous by design |
| `POST /api/v1/shares`, `DELETE /api/v1/shares/{id}` | Yes |
| `GET /api/v1/shares/{id}`, `GET /api/v1/shares/{id}/image` | No, share links are public |

## Accounts

`POST /api/v1/auth/register` creates an account from an email and a password. The email is trimmed and lower-cased, so `Ona@Example.test ` and `ona@example.test` are the same account. The password must be 10 to 128 characters and not blank. It is stored as a salted PBKDF2 hash (ASP.NET Core Identity format) and never logged.

Registering an email that already exists answers `409 Conflict`.

## Tokens

`POST /api/v1/auth/login` returns a JSON Web Token signed with HMAC SHA-256.

| Claim | Value |
|---|---|
| `iss` | `places-api` |
| `aud` | `places-app` |
| `sub` | The account id |
| `exp` | 30 days after issue |

Send the token in the `Authorization` header on every request that needs it.

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

The server accepts up to 30 seconds of clock difference. There is no refresh token in version 1. When `expiresAt` has passed, sign in again.

## Failures

| Situation | Status | Body |
|---|---|---|
| Wrong email or wrong password | 401 | `"detail": "Email or password is incorrect."` |
| Missing, malformed, expired or wrongly signed token | 401 | `"title": "Unauthorized"`, plus a `WWW-Authenticate: Bearer` header |
| Valid token, but the share belongs to someone else | 404 | The same body as a share that does not exist |

Wrong email and wrong password give the same answer on purpose, so nobody can find out which emails are registered. For the same reason another person's share answers 404, not 403.

## Security notes

1. Always call the API over HTTPS outside local development. The token is a bearer credential, anyone who has it can act as the user until it expires
2. On Android keep the token in encrypted storage, never in plain shared preferences or logs
3. Never put the token or the password in a URL. Query strings end up in proxy and server logs
4. The signing key lives only in the server environment (`JWT_SIGNING_KEY`). It is never committed. Rotating it signs every user out
5. Feedback endpoints ignore the token if one is sent and store no account id, so feedback cannot be tied back to a person

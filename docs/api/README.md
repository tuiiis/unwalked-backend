# Unwalked API documentation

The Unwalked API serves the place catalogue to the mobile app, keeps public links for shared achievements and collects anonymous feedback on recommendations. Location history never leaves the phone. The server only ever sees map cells the app asks about.

## Where to start

| If you want to | Read |
|---|---|
| Run the API and make a first request in five minutes | [Getting started](getting-started.md) |
| Build the full sign in and share flow | [Tutorial: share an achievement](tutorials/share-an-achievement.md) |
| Show places around a user's usual streets | [Tutorial: find places near habitual areas](tutorials/places-near-habitual-areas.md) |
| Look up an endpoint, parameter or schema | [Reference](reference/README.md) or Swagger UI at `/swagger` |
| Understand how the API fits into the system | [Architecture](architecture.md) |

## Guides

| Guide | Content |
|---|---|
| [Authentication](guides/authentication.md) | Accounts, tokens, lifetime, what to keep secret |
| [Errors](guides/errors.md) | Problem details format, every status code and how to recover |
| [Limits and paging](guides/limits-and-paging.md) | Result limits, area limits and why there is no cursor paging |
| [Idempotency](guides/idempotency.md) | Safe retries for feedback |
| [Versioning](guides/versioning.md) | URL versioning, what counts as a breaking change, deprecation |

## Specification

[openapi.yaml](openapi.yaml) is the OpenAPI 3.0 specification. It is generated from the code and checked by an integration test, so it cannot drift from the running API. Every operation carries request and response examples captured from a local run on the Vilnius catalogue.

The running API also serves it at `/openapi/v1.yaml` and `/openapi/v1.json`.

## About this documentation

[Documentation strategy](documentation-strategy.md) describes the tools, conventions and known gaps. Changes are listed in the [changelog](CHANGELOG.md).

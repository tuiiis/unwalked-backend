# Documentation strategy

## Audience

The main reader is a developer who builds or maintains a client for the API, today the Unwalked Android app. A second reader is a reviewer who needs to understand what the API does without running it. Both read on GitHub, so everything is plain Markdown with Mermaid diagrams that GitHub renders.

## Sources of truth

| What | Source | How it stays correct |
|---|---|---|
| Endpoints, parameters, schemas, status codes | Controller attributes and contracts in C# | Swashbuckle generates the specification at runtime |
| Examples | `ExamplesFilter` in the API project | Captured from a local run on the Vilnius catalogue, error messages copied from the domain code |
| Committed specification | `docs/api/openapi.yaml` | An integration test compares it with the served document and fails on any difference |
| Guides and tutorials | Markdown in `docs/api` | Every command in a tutorial was run against a local instance before it was written down |

Generating the specification from code means a renamed parameter cannot be forgotten in the docs. The committed copy exists so the specification is readable and diffable without starting the server.

## Tools

| Tool | Use |
|---|---|
| Swashbuckle 9 | Generates OpenAPI 3.0 from the code, serves Swagger UI at `/swagger` |
| Spectral 6 (`spectral:oas` ruleset) | Lints the specification, configured in `.spectral.yaml` |
| Redocly CLI 2 | Second validator, and renders a static Redoc page on demand |
| NUnit + Testcontainers | The drift test runs with the rest of the integration tests |
| Mermaid | Context, layer and sequence diagrams inside Markdown |

Validate locally with either linter.

```bash
npx @stoplight/spectral-cli@6.15.0 lint docs/api/openapi.yaml
npx @redocly/cli@2.11.1 lint docs/api/openapi.yaml
```

Regenerate the committed specification after changing the API.

```bash
UPDATE_OPENAPI=1 dotnet test tests/Unwalked.IntegrationTests --filter FullyQualifiedName~OpenApiDocumentTests
```

## Structure

```text
docs/api/
  README.md                  entry point and table of contents
  getting-started.md         first request in five minutes
  architecture.md            context, layers, request lifecycle
  openapi.yaml               generated specification
  guides/                    one topic per file: auth, errors, limits, idempotency, versioning
  tutorials/                 end-to-end tasks with real commands and outputs
  reference/                 a map of the specification
  documentation-strategy.md  this file
  CHANGELOG.md               changes per version
```

The split follows the Diátaxis idea: tutorials teach, guides explain one concern, the reference describes. A reader who knows what they want goes to the reference, a newcomer starts at Getting started.

## Conventions

| Area | Rule |
|---|---|
| Paths | Lower case, plural nouns, kebab case: `/places/in-bbox` |
| Operation ids | Camel case verbs: `findPlacesNear`, `createShare` |
| JSON fields | Camel case: `distanceMeters`, `achievedOn` |
| Enum values on the wire | Snake case: `nearby_new_place` |
| Units | In the name when not obvious: `distanceMeters`, `radius` documented in metres |
| Dates | ISO 8601. `date` for days, `date-time` with offset for instants |
| Coordinates | WGS84 degrees, latitude before longitude, `lat` and `lon` |
| Examples | Real values from Vilnius, never `string` or `foo` |
| Error messages | One sentence, ends with a full stop, names the limit |
| Docs prose | Short sentences, second person, present tense, no screenshots that go stale |

## Versioning

Refer to [Versioning](guides/versioning.md). In short: the major version is in the path, the specification uses semantic versioning, every change goes into the changelog.

## Known gaps

1. There is no hosted documentation site yet. The reference is read on GitHub or through Swagger UI on a running instance. A deployed instance will serve Swagger UI publicly once the Cloud criterion is done
2. Rate limiting is described as planned, it is not implemented in version 1
3. There are no contract tests between the Android client and the API. The drift test protects the specification, not the client
4. Examples are maintained by hand in code. They are checked against real output when written but nothing re-checks them automatically
5. Token refresh is not supported, so the authentication guide only covers signing in again

---
appliesTo:
  - "KickRoll.Api/**"
---

# API Instructions (KickRoll.Api)

## Scope
Rules apply to the ASP.NET Core Web API only. Keep web concerns separate from MAUI client.

## Build & Run
```bash
dotnet build KickRoll.Api/KickRoll.Api.csproj -c Debug
dotnet run --project KickRoll.Api/KickRoll.Api.csproj --launch-profile "http"
```
- Use environment variables / appsettings for configuration; do not hardcode secrets.

## Architecture
- Minimal APIs or Controllers are acceptable; group endpoints by feature area.
- Separate layers: Endpoints ↔ Services ↔ Data/External. Avoid business logic in endpoints.
- Use DTOs; avoid exposing domain entities directly.

## Conventions
- JSON only; `camelCase` property names with `System.Text.Json`.
- HTTP semantics: 2xx for success, 4xx client errors, 5xx server errors.
- Return `ProblemDetails` for errors; never leak stack traces.

## Validation
- Validate all request models using `DataAnnotations` or FluentValidation.
- Reject invalid input with 400 + validation details.

## Auth & Security
- If auth is added: use JWT Bearer; require `[Authorize]` where needed and `[AllowAnonymous]` sparingly.
- CORS: allow only known app origins; avoid `AllowAnyOrigin` in Release.
- Use HTTPS-only; redirect HTTP→HTTPS in production.

## Logging & Observability
- Use `ILogger<T>` and structured logs. Add correlation IDs for multi-service operations.
- Expose health checks at `/healthz`.

## Swagger
- Enable Swashbuckle in Development. Keep docs up-to-date with summaries and example payloads.

## Data & External Calls
- Wrap external HTTP calls in typed clients with Polly retry (idempotent only). Timeouts are mandatory.
- For persistence, add migrations and versioning; no destructive schema changes without migration.

## Versioning
- Version the API route (`/v1/...`). Breaking changes require new version and deprecation note.

## Performance
- Prefer async all the way; avoid sync-over-async.
- Cache hot reads where sensible (MemoryCache or response caching).

## Testing
- Unit test services and mappers; integration tests for endpoints (WebApplicationFactory) if available.
- Ensure `dotnet test` passes; include tests when you change behaviors.

## PR Checklist (API)
- Endpoints documented in Swagger (summaries/examples).
- Validation and error handling covered.
- Logging added where it helps diagnosis.
- Tests pass; CI green.
- Security review (CORS/auth/secrets) acknowledged in PR description.

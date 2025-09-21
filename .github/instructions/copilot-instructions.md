# Repository custom instructions for Copilot (KickRoll)

> Audience: GitHub Copilot coding agent & chat on GitHub.com  
> Goal: Make the agent ship **tested PRs** for this .NET MAUI app (KickRoll.App) and companion API (KickRoll.Api) with minimal back-and-forth.

---

## High-level repo context
- **Solution**: `KickRoll.sln`
- **Projects**:
  - `KickRoll.App` — **.NET MAUI** cross‑platform app (Android/iOS/Windows/Mac Catalyst). UI is XAML + MVVM; composition via `MauiProgram.cs`.
  - `KickRoll.Api` — **ASP.NET Core Web API** (REST). Prefer minimal APIs where practical.
- **Language**: C# 12 / **.NET 8 LTS** (assume unless repo states otherwise). If tooling detects a different TFM, follow the project file.
- **Package manager**: `dotnet` CLI. Keep lock files updated if enabled.
- **Branching**: Create feature branches per issue. Do **not** push directly to `master`/`main`.

## Build, run & validate
> Always run **restore → build → test** locally (in the VM) before opening a PR.

### Common setup
```bash
dotnet --info
dotnet restore KickRoll.sln
```

### Build
```bash
# Build entire solution (Debug by default)
dotnet build KickRoll.sln -warnaserror

# Release build
dotnet build KickRoll.sln -c Release -warnaserror
```

### Test
- If tests exist, run them:
```bash
dotnet test KickRoll.sln -c Release --collect:"XPlat Code Coverage"
```
- If **no test projects exist yet**, generate reasonable unit tests for touched code (xUnit), then run them. Treat lack of tests as a **gap to fix** in the PR.

### Lint & format
```bash
dotnet format --verify-no-changes || dotnet format
```
- Respect `.editorconfig`. Keep nullable enabled and avoid `dynamic`/`object` unless necessary.

## MAUI app (KickRoll.App) specifics
- **Architecture**: MVVM; Views are XAML with code-behind only for wiring. Business logic goes into ViewModels/Services. Use `ICommand` for actions.
- **Entry**: `MauiProgram.cs` configures DI, fonts, handlers. Prefer registering services as interfaces.
- **Resources**: Put colors, styles, and font registrations in `Resources/` and `App.xaml`. **Do not** hardcode styles in views.
- **Navigation**: Use Shell (`AppShell.xaml`) if present; otherwise centralize routes in a single routing service.
- **Platforms**: Platform-specific code under `Platforms/Android`, `Platforms/iOS`, etc. Do not add platform code into shared project unless using partials/`#if`.
- **Permissions**: Request via `MainApplication`/`Info.plist`/`AndroidManifest.xml` as applicable. Add runtime checks and graceful fallbacks.
- **Performance**: Enable trimming & AOT where safe for Release. Avoid reflection-heavy patterns in hot paths.
- **CI sanity checks**: Ensure Android/Windows builds on CI. If iOS not available in CI, still compile with `-f:net8.0-ios` to catch API breaks when possible.

### MAUI run targets (use these when implementing features or fixing issues)
```bash
# Android emulator/device (adjust RID per installed workloads)
dotnet build KickRoll.App/KickRoll.App.csproj -c Debug -f net8.0-android
# Windows
dotnet build KickRoll.App/KickRoll.App.csproj -c Debug -f net8.0-windows10.0.19041.0
# Mac Catalyst (if enabled)
dotnet build KickRoll.App/KickRoll.App.csproj -c Debug -f net8.0-maccatalyst
```

> If the project uses a different TFM (e.g., net7.0-android), infer from the `.csproj` and build with that.

## API (KickRoll.Api) specifics
- **Run locally**:
```bash
dotnet run --project KickRoll.Api/KickRoll.Api.csproj --launch-profile "http"
```
- Prefer **Minimal APIs** (or standard Controllers) with clear DTOs and validation using `FluentValidation` or `DataAnnotations`.
- Enable **Swagger/Swashbuckle** for local testing if not present.
- **Error handling**: Use ProblemDetails; never return raw exceptions.
- **Logging**: Use `ILogger<T>`; structure logs with event IDs where critical.
- **CORS**: Restrict to app origins; do not use `AllowAnyOrigin` in Release.

## Pull request rules (for the agent)
- Open PRs that:
  - Link the issue (e.g., `Closes #123`), include **summary**, **risk**, **test plan**, **rollback**.
  - Include **unit tests** for new code and critical regressions.
  - Keep changes scoped; avoid large refactors unless explicitly asked.
  - Update MAUI resources/strings when UI changes.
- If build breaks on any target platform, **fix or downgrade the scope** and explain constraints.

## Safety rails & constraints
- **Do not** modify files under `/Platforms/*` unless the change is platform-necessary (permissions, manifests, entitlements).
- **Do not** introduce new dependencies without justification and size/security impact (Android APK size matters).
- Keep backwards‑compatible migration paths for settings/storage. If schema changes (SQLite/Preferences), add migration code.
- Avoid using reflection-based JSON serializers that break AOT; prefer `System.Text.Json` with source-gen if heavy usage.

## Useful scripts (have the agent create if missing)
- `./scripts/ci-build.ps1|sh` — restore + build + test + format check.
- `./scripts/run-api.ps1|sh` — runs API with seed data for local testing.
- `./scripts/emulator-android.ps1|sh` — ensures emulator is up before build.

## Issue templates the agent expects
- **Bug**: steps to reproduce, expected vs actual, device/OS, screenshots/logs.
- **Feature**: user story, acceptance criteria, non‑goals, UI mocks (if any).
- **Tech debt**: rationale, measurable outcome, risk, test impact.

## Definition of done (agent checklist)
- Code builds on `dotnet build KickRoll.sln -c Release`.
- Tests pass with useful coverage on changed code.
- App/Api can **run** locally (or platform-simulated build succeeds).
- PR description includes summary, risks, test plan, rollback.
- No formatting/lint violations.

---

### Path-specific guidance (optional)
You can add more granular files under `/.github/instructions`:

`/.github/instructions/app.instructions.md`
```md
---
applyTo:
  - "KickRoll.App/**"
---
# MAUI UI guidelines
- Favor XAML bindings and `DataTemplate` reuse.
- Put transient UI state in ViewModel; avoid singletons for page state.
- Respect accessibility (AutomationProperties, contrast, font scaling).
```

`/.github/instructions/api.instructions.md`
```md
---
applyTo:
  - "KickRoll.Api/**"
---
# API guidelines
- Group endpoints by feature area.
- Use typed `HttpClient` for external calls; add Polly retry where needed.
- Validate all inputs; return ProblemDetails on errors.
```

---

## What to do when assigned an issue (for Copilot)
1. Read linked issue and repo instructions.
2. Plan changes (files to edit, tests to add). Comment the plan.
3. Create a feature branch `feature/<short-desc>`.
4. Implement with small commits; keep commit messages meaningful.
5. Run build/tests/format. If any fail, fix before PR.
6. Open PR with full description and link to the issue.
7. Respond to review comments and iterate until green.


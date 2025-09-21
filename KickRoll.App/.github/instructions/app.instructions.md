---
appliesTo:
  - "KickRoll.App/**"
---

# MAUI App Instructions (KickRoll.App)

## Scope
Rules apply to the .NET MAUI client app only. Keep platform-specific code inside `Platforms/*` or partial classes.

## Build & Run
```bash
# Build common targets (adjust TFM if project file differs)
dotnet build KickRoll.App/KickRoll.App.csproj -c Debug -f net8.0-android
dotnet build KickRoll.App/KickRoll.App.csproj -c Debug -f net8.0-windows10.0.19041.0
# Optional
dotnet build KickRoll.App/KickRoll.App.csproj -c Debug -f net8.0-maccatalyst
```
- Prefer Release builds for performance verification.
- If a target framework mismatch is detected in the `.csproj`, use that TFM.

## Architecture & Patterns
- MVVM: Views (XAML) ↔ ViewModels via bindings. Keep code-behind minimal (wiring only).
- DI in `MauiProgram.cs`; register services/interfaces with proper lifetimes (Singleton for config, Transient for view services).
- Centralize navigation via Shell (`AppShell.xaml`) or a dedicated NavigationService. Avoid ad-hoc `Navigation.PushAsync` in Views.
- Keep business logic out of Views; use `ICommand` and async commands.

## UI & Resources
- Define colors, typography, and styles in `App.xaml` / `Resources/Styles`.
- Reuse `DataTemplate`s and `ControlTemplate`s to prevent duplication.
- Use `StaticResource` for shared values; avoid hard-coded sizes/colors in XAML.
- Strings go to resx; avoid literals in Views/ViewModels for localizable text.

## State & Storage
- Prefer immutable ViewModel state; expose `ObservableProperty` or `INotifyPropertyChanged` patterns.
- Use `Preferences`/`SecureStorage` for simple key‑values; for structured data use SQLite with migrations.

## Networking
- Put HTTP logic behind typed services (e.g., `IApiClient`); do not call `HttpClient` directly from Views.
- Implement cancellation tokens for long-running calls.

## Performance
- Enable trimming/AOT in Release where safe.
- Use `OnAppearing` for lightweight work; heavy tasks should be backgrounded with progress feedback.
- Avoid reflection-heavy libraries that cause AOT issues.

## Accessibility
- Set `AutomationProperties.Name` on actionable controls.
- Respect dynamic font scaling and contrast; do not fix font sizes without reason.

## Platforms
- Android permissions in `AndroidManifest.xml`; iOS entitlements in `Info.plist`. Always include runtime checks.
- Platform-specific features belong under `Platforms/<platform>/` (use partial classes or conditional compilation).

## Telemetry & Logging
- Use `ILogger<T>` for diagnostics; no `Console.WriteLine` in production paths.

## Testing
- Unit test ViewModels and services (xUnit). Avoid UI-thread coupling in tests.
- If no tests exist for edited code, create them. Ensure `dotnet test` runs green before PR.

## PR Checklist (App)
- Screenshots/gif for UI changes.
- New/updated resources (styles/strings) checked in.
- Build succeeds for at least one mobile target plus Windows (if applicable).
- Tests added/updated for changed logic.
- No formatting errors (`dotnet format`).


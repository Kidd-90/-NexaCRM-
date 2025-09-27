# NexaCRM.WebServer Hosting Notes

## Purpose
- Capture operational considerations for running the NexaCRM server-side Blazor host.
- Document guard rails that keep the host online even when external dependencies (such as Supabase) are unavailable.

## Supabase configuration fallback
- `SupabaseClientOptions` now exposes settable properties so post-configuration hooks can supply deterministic offline defaults when no secrets are supplied.
- `AddSupabaseClientOptions` posts config values that fall back to `https://localhost` and a deterministic anon key when `validateOnStart` is disabled. This prevents `OptionsValidationException` from failing the build or runtime startup while still validating real values when supplied.
- Both the server (`Startup.ConfigureServices`) and WebAssembly host (`Program`) opt into the relaxed validation path and log a warning when offline defaults are in effect. This keeps duplicate monitor services and other hosted background tasks from faulting during dependency injection.
- Blazor UI services that rely on Supabase continue to receive a client instance. Runtime operations that require the real backend will fail gracefully while the application shell keeps rendering.

## Duplicate monitor resilience
- `Startup.Configure` registers the duplicate monitor inside an asynchronous scope. Any exception thrown during startup is now caught and logged instead of crashing the host.
- Logging uses the shared ASP.NET Core logging infrastructure so the failure reason surfaces in console output and any centralized log sinks.

## Authentication middleware guard
- The request pipeline now inspects `IAuthenticationSchemeProvider` before invoking `UseAuthentication`/`UseAuthorization`.
- When no authentication schemes are registered (the current default for the server host), the middleware is skipped and an informational log entry is emitted. This prevents runtime crashes caused by an empty authentication configuration.

## Operational checklist
- Confirm Supabase secrets are supplied through `appsettings.json`, environment variables, or user secrets before enabling real sign-in flows.
- Monitor logs on startup to verify whether the duplicate monitor and authentication middleware were activated.

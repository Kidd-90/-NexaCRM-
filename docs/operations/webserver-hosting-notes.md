# NexaCRM.WebServer Hosting Notes

## Purpose
- Capture operational considerations for running the NexaCRM server-side Blazor host.
- Document guard rails that keep the host online even when external dependencies (such as Supabase) are unavailable.

## Supabase configuration fallback
- `Startup.ConfigureServices` now calls `AddSupabaseClientOptions` with `validateOnStart: false`, allowing the site to boot even when Supabase secrets are absent in the configuration source.
- The scoped `Supabase.Client` factory checks whether both the Supabase URL and anon key are present. When either is missing, the factory logs a warning and swaps in a loopback endpoint (`https://localhost`) with a deterministic key so the DI graph is satisfied.
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

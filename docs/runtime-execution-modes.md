# Runtime Execution Mode Strategy

## Overview
NexaCRM currently ships as a Blazor WebAssembly application. The user experience and UI components must remain unchanged while we gain the flexibility to host the same code base either on the client (WebAssembly) or on the server (Blazor Server or other server-rendered hosts). This document describes the dependency-injection structure that enables runtime selection without duplicating feature code.

## Goals
- Keep Razor components, services, and UI assets identical across execution modes.
- Centralise shared service registrations so behaviour is consistent regardless of host.
- Encapsulate environment-specific wiring (HTTP base address, Supabase session storage, realtime configuration) so it can be swapped without touching UI code.
- Provide an explicit extension point for future server hosting scenarios, including integrations that require ASP.NET Core services.

## Current Challenges
The previous `Program.cs` mixed all service registrations directly into the WebAssembly bootstrapping logic. That made the project tightly coupled to WebAssembly specific features such as browser-based Supabase session persistence and the `HttpClient` that depends on `HostEnvironment.BaseAddress`. Any attempt to run the same assemblies in a server process required editing that file, risking regressions for the client build.

## Project Layout
The UI surface now lives in a Razor class library (`src/Web/NexaCRM.WebClient`). Two thin host projects reference that library while supplying the runtime-specific bootstrapping code:

| Project | Responsibility |
| --- | --- |
| `NexaCRM.WebClient` | Shared Razor components, pages, services, resources, and static assets. |
| `NexaCRM.WebClient.WasmHost` | WebAssembly entry point (`Program.cs`, `wwwroot/index.html`, WebAssembly-specific configuration). |
| `NexaCRM.WebClient.ServerHost` | Server entry point with ASP.NET Core pipeline, session-backed Supabase persistence, and named `HttpClient` configuration. |

Both hosts reference the same UI assembly, so any component or styling change immediately applies to each runtime without duplication.

## Service Registration Layers
To decouple environment concerns, the following extension methods remain central in `DependencyInjection/NexaCrmServiceCollectionExtensions.cs`:

| Method | Purpose |
| --- | --- |
| `AddNexaCrmCoreServices(IConfiguration)` | Registers all shared services, options, and state providers that are agnostic to the hosting model. |
| `AddNexaCrmWebAssemblyRuntime(WebAssemblyHostBuilder)` | Applies client-specific dependencies: browser `HttpClient`, Supabase session persistence via `localStorage`, and a Supabase client configured for WebAssembly limitations. |
| `AddNexaCrmServerRuntime(Action<NexaCrmServerRuntimeOptions>)` | Provides a single entry point for supplying server-specific factories such as a server friendly `HttpClient`, session persistence, and Supabase client behaviour. |

Each host project's `Program.cs` chains these methods so that all environment-neutral logic stays in one place and runtime specific behaviour is attached explicitly when building the host. This separation allows us to reuse the same components for other host types simply by calling the appropriate extension.

### Core Service Set
The core extension consolidates the registrations that were previously embedded in `Program.cs`, including:
- `AddAuthorizationCore()`, localisation, and cascading authentication state.
- Supabase client configuration binding.
- All domain service bindings (e.g., `IContactService`, `IStatisticsService`) and supporting singletons such as `SupabaseEnterpriseDataStore`.
- Authentication plumbing (`CustomAuthStateProvider`, `SupabaseClientProvider`, `ActionInterop`).

Keeping these together ensures both WebAssembly and server hosts obtain the same service graph without duplication.

### WebAssembly Runtime
`AddNexaCrmWebAssemblyRuntime` mirrors the previous WebAssembly-specific setup. It creates the browser-based `HttpClient`, wires `SupabaseSessionPersistence`, and instantiates `Supabase.Client` with realtime disabled to avoid the WebSocket limitations on WebAssembly. Because this logic lives behind an extension method, the UI project stays readable while continuing to compile exactly as before.

### Server Runtime Options
Server hosting can require different infrastructure—for example, storing Supabase sessions in distributed cache or cookies, and using `IHttpClientFactory`. The new `NexaCrmServerRuntimeOptions` type lets server hosts describe those dependencies via factories. Validation ensures all required pieces are supplied, keeping build-time safety. The dedicated server host (`NexaCRM.WebClient.ServerHost`) wires these requirements up:

```csharp
builder.Services
    .AddNexaCrmCoreServices(builder.Configuration)
    .AddNexaCrmServerRuntime(options =>
    {
        options.HttpClientFactory = sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("NexaCRM.Api");
        options.SessionPersistenceFactory = sp => sp.GetRequiredService<ServerSupabaseSessionPersistence>();
        options.SupabaseClientFactory = sp =>
        {
            var supabaseOptions = sp.GetRequiredService<IOptions<SupabaseClientOptions>>().Value;
            return new Supabase.Client(supabaseOptions.Url, supabaseOptions.AnonKey);
        };
        options.ConfigureServices = services =>
        {
            services.AddScoped<ServerSupabaseSessionPersistence>();
            services.AddHttpClient("NexaCRM.Api", (sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["ApiGateway:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
        };
    });
```

`ServerSupabaseSessionPersistence` (implemented in the server host) persists Supabase sessions into ASP.NET Core session state so interactive connections remain authenticated across requests. Because the type lives only in the server host, the WebAssembly build stays focused on browser persistence strategies.

## Migration Checklist
1. Move any additional shared registrations into `AddNexaCrmCoreServices` so they are reused automatically.
2. For WebAssembly, ensure `Program.cs` only calls the core and WebAssembly extensions (already implemented).
3. When adding a server host project, implement a persistence strategy and `HttpClient` factory, then call `AddNexaCrmServerRuntime` during startup.
4. Validate Supabase configuration using existing options binding—no code changes required in UI components.
5. Keep automated builds (`dotnet build --configuration Release`) and tests (`dotnet test ./tests/BlazorWebApp.Tests --configuration Release`) in CI to guarantee both registration paths compile.

## Testing Strategy
- Continue running `dotnet build --configuration Release` to ensure the UI project and its dependencies compile after registration changes.
- Execute `dotnet test ./tests/BlazorWebApp.Tests --configuration Release` to confirm unit tests (including Supabase option validation) pass regardless of host configuration.

## Technology Tracking
- **Blazor**: UI rendering technology shared across WebAssembly and server hosts.
- **Supabase**: Backend-as-a-service used for authentication, realtime, and data access. Runtime configuration dictates session persistence.
- **Dependency Injection (Microsoft.Extensions.DependencyInjection)**: Primary mechanism for sharing code across runtime environments.

Maintaining this document keeps the runtime strategy alongside the rest of our technical documentation, satisfying the requirement to track employed technologies in Markdown.

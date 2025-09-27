# NexaCRM Web Server Documentation

## Project Overview
- **Framework**: Blazor Web App (Interactive Server + WebAssembly render modes)
- **Runtime**: .NET 8 (C# 12) with localization configured for `ko-KR`
- **Purpose**: Host the NexaCRM UI components on the server while matching the WebClient experience without modifying the client project.

## Technology Stack
| Component | Technology | Notes |
|-----------|------------|-------|
| Frontend Host | ASP.NET Core Blazor Web App | Serves interactive components and static web assets |
| Shared UI | `NexaCRM.UI` Razor Class Library | Provides layouts, pages, and shared styling resources |
| Localization | `RequestLocalizationOptions` with `ko-KR` | Ensures consistent formatting across server-rendered pages |
| Configuration | `SupabaseClientOptions` (bound via options pattern) | Keeps server aligned with WebClient configuration validation |

## Design Alignment Strategy
1. **Shared Static Assets**: `wwwroot/index.html` mirrors the WebClient loader and references `_content/NexaCRM.UI` CSS/JS bundles.
2. **Head Resources**: `<HeadContent>` from `MainLayout` renders CDN and shared styles; `HeadOutlet` in `App.razor` ensures delivery on the server.
3. **Namespace Imports**: `_Imports.razor` now mirrors the WebClient usings so layout/components resolve identically without touching WebClient code.
4. **Service Registration**: `Program.cs` aligns DI with the WebClient by loading NexaCRM admin services, mock interaction services, and validating Supabase options.

## Localization & Culture
- Default culture and UI culture are forced to `ko-KR`.
- Request localization middleware is configured with `ko-KR` as the only supported culture to keep formatting deterministic.

## Reliability Improvements
- Startup duplicates monitor errors are now logged instead of crashing the app.
- Supabase configuration binding validates at startup, surfacing missing keys early.

## Static Asset Notes
- `_framework/blazor.web.js` bootstraps the interactive Blazor runtime.
- Loader markup in `index.html` matches WebClient to provide identical first paint UX.
- Shared scripts (auth, navigation, theme, device, interactions, CSV export) are loaded from the UI library so changes propagate to both hosts automatically.

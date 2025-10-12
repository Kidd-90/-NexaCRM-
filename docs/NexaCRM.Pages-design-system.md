# NexaCRM.Pages Design System Assets

## Overview
- **Design reference**: Pipedrive Web Application Design Blueprint JSON (`DesignSystem/PipedriveDesignSystem.json`).
- **Purpose**: Capture design philosophy, layout guidance, and interaction flows so AI tools and developers can reproduce the Pipedrive-inspired UI consistently.
- **Key capabilities**:
  - Structured design tokens for colors, typography, and tone of voice.
  - Layout definitions covering grid, navigation tiers, and signature views.
  - Component catalog with behavioral notes for high-traffic UI pieces.
  - User flow breakdowns and AI implementation considerations.
  - Extended modern & minimal palette documenting both light and dark roles with usage guidance.

## Razor Components
To encourage reuse, the following Razor components are available under `Components/DesignSystem`:

| Component | Description | Typical Usage |
| --- | --- | --- |
| `ColorSwatch` | Renders a labeled color preview with optional description. | Show color tokens in documentation or design review screens. |
| `PaletteRoleList` | Visualizes the light/dark palette roles with swatch, hex, and usage copy. | Communicate theming tokens to designers, AI prompts, or admin tools. |
| `TypographySample` | Displays a type scale entry using the configured size/line-height. | Present typography tokens or style guides in admin tooling. |
| `PrincipleList` | Lists guiding principles with accent markers. | Summaries of philosophy, tone, or experience goals. |

## Layout & Authentication Helpers
- `Components/Layout/MainLayout.razor`
  - Two-column workspace chrome that reflects the neutral gray + royal blue palette.
  - Provides responsive sidebar toggling, breadcrumb, and primary action affordances without depending on `NexaCRM.UI`.
  - Uses the shared `wwwroot/js/layout.js` module to initialise theme state and persist the light/dark preference.
- `Components/Shared/LoadingScreen.razor`
  - Lightweight Tailwind-based loading indicator for both Server and WASM hosts.
  - Designed as a drop-in replacement for the Bootstrap spinner previously shipped in `NexaCRM.UI`.
- `Components/Shared/RedirectToLogin.razor`
  - Preserves the login redirect workflow while logging errors for diagnostics.
  - Emits the new loading screen so unauthenticated users see a consistent experience while navigation occurs.

## Authentication Pages
- `Pages/Auth/AuthPortal.razor`
  - Serves `/login`, `/register`, and `/forgot-password` routes with a shared minimal two-column layout that mirrors the modern neutral palette.
  - Marked with `[AllowAnonymous]` and `@layout null` so the experience bypasses the workspace shell while remaining compatible with the router's anonymous-route detection.
  - Includes inline OAuth launch buttons and Tailwind-friendly form styling driven by the co-located `AuthPortal.razor.css` scoped stylesheet.
  - Utilises `NavigationManager.LocationChanged` to hot-swap form content without reloading the shell, keeping validation state local to each flow.

- `Pages/Auth/AuthPortal.razor.css`
  - Reuses the shared `app-shell` design tokens (`--nexa-*`) instead of redefining palette variables, preventing drift with existing components.
  - Styles reusable classes (`themed-body`, `btn-primary`, `social-btn`, `form-input`) that the Razor page references without mutating existing shared components.
  - Keeps button focus states and animations accessible, using `color-mix` for subtle glow effects that align with the Tailwind build pipeline.

Each component is Tailwind-friendly, so utility classes applied in host apps will compose correctly after building shared CSS.

## Consuming the Blueprint
```csharp
using System.Linq;
using NexaCRM.Pages.DesignSystem;

var blueprint = DesignSystemBlueprintProvider.Blueprint;
var palette = blueprint.BrandIdentity.ColorPalette;

var accentPrimary = palette.LightMode.Roles.First(role => role.Token == "accentPrimary");
var darkBackground = palette.DarkMode.Roles.First(role => role.Token == "primaryBackground");
```

- `PaletteRoleList` pairs naturally with `palette.LightMode.Roles` and `palette.DarkMode.Roles` to render the modern neutral + blue design tokens.
- Surface `palette.StyleGuide` keywords and reference brands when generating AI prompts so outputs remain aligned with the neutral gray + blue aesthetic.

- The blueprint JSON is embedded as a resource and loaded lazily via `DesignSystemBlueprintProvider`.
- All models are strongly typed with nullable annotations enabled to ensure build-time safety.

## Maintenance Notes
- Update `PipedriveDesignSystem.json` when new tokens or flows are introduced. Keep descriptions action-oriented to help AI prompts.
- When adding reusable visual patterns, consider creating a dedicated Razor component alongside documentation updates.
- Run `dotnet build` for the solution to verify the resource is embedded and the Razor components compile.

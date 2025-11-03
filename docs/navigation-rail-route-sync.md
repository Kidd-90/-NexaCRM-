# NavigationRail Route Synchronization

## Overview
The `NavigationRail` component in the UI shell now keeps its icon rail and detail panel synchronized with the current router state. A shared lookup table that maps each accessible navigation link to its parent group ensures the correct icon is marked active and the matching section in the detail panel is expanded.

## Key Behaviors
- The component normalizes route URIs by trimming query strings and fragments before matching them to navigation links.
- Whenever navigation data is rebuilt (for example after role changes) the lookup table is refreshed to reflect the links available to the signed-in user.
- The `NavigationManager.LocationChanged` event triggers a sync so direct URL entry, browser navigation, or programmatic redirects keep the rail selection accurate.
- Fallback logic resolves catalog definitions when the current URI is an alias (such as the empty root path) to keep the dashboard group selected without relying on UI defaults.

## Responsive Behavior
- A `matchMedia`-driven interop watches mobile, tablet, desktop, and widescreen breakpoints to decide when the navigation panel should overlay content versus remain pinned.
- Breakpoint-specific CSS variables (`--nav-tail-panel-width`, `--nav-tail-rail-width`) adjust the rail width and panel footprint, allowing tablet layouts to use narrower overlays while large desktops expand the detail panel.
- Header actions (such as the close affordance) are toggled at `max-width: 1279.98px` so compact viewports expose a dismiss button while widescreen layouts keep the panel permanently docked.

## Technology Notes
- Implemented with Blazor Server-side Razor component features targeting .NET 8.
- Utilizes `NavigationManager` for URI parsing and `NavigationCatalog` for centralized navigation metadata.
- Applies C# 12 features (such as primary constructors for records) already in use across the project to maintain consistency.
- Uses `window.matchMedia` via a dedicated ES module (`navigationTailInterop.js`) to publish viewport category changes back to Blazor and persists per-breakpoint state into a JSON payload stored under the `nexacrm.activeSubMenuState.v2` key.

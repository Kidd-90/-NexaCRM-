# NavigationRail Route Synchronization

## Overview
The `NavigationRail` component in the UI shell now keeps its icon rail and detail panel synchronized with the current router state. A shared lookup table that maps each accessible navigation link to its parent group ensures the correct icon is marked active and the matching section in the detail panel is expanded.

## Key Behaviors
- The component normalizes route URIs by trimming query strings and fragments before matching them to navigation links.
- Whenever navigation data is rebuilt (for example after role changes) the lookup table is refreshed to reflect the links available to the signed-in user.
- The `NavigationManager.LocationChanged` event triggers a sync so direct URL entry, browser navigation, or programmatic redirects keep the rail selection accurate.
- Fallback logic resolves catalog definitions when the current URI is an alias (such as the empty root path) to keep the dashboard group selected without relying on UI defaults.

## Technology Notes
- Implemented with Blazor Server-side Razor component features targeting .NET 8.
- Utilizes `NavigationManager` for URI parsing and `NavigationCatalog` for centralized navigation metadata.
- Applies C# 12 features (such as primary constructors for records) already in use across the project to maintain consistency.

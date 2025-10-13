# Layout Navigation Componentization

## Overview
The `MainLayout` navigation experience is now composed from dedicated Blazor components so each concern stays isolated while the layout preserves its themed styling.

## Components

### `PrimaryNavRail`
- **Purpose**: Renders the icon-based primary navigation rail and raises selection events for the layout.
- **Technologies**: Blazor component (`.razor`) targeting .NET 8, relies on `RenderFragment` to stream SVG icons.
- **Parameters**:
  - `Sections`: Ordered list of `PrimaryNavSection` values that should appear in the rail.
  - `ActiveSection`: Highlights the active section with the shared accent token.
  - `Navigation`: Provides localized labels for accessibility.
  - `OnSectionSelected`: Event callback invoked when a section is chosen.

### `SecondaryNavPanel`
- **Purpose**: Presents the contextual links for the active primary section and handles the collapse animation width toggling.
- **Technologies**: Blazor component (`.razor`) consuming `NavLink` from `Microsoft.AspNetCore.Components.Routing` to keep router state in sync.
- **Parameters**:
  - `IsOpen`: Controls panel width and overflow.
  - `ActiveSection`: Determines which secondary link set to render.
  - `ActiveSectionLabel`: Displays the header label above the link list.
  - `Navigation`: Supplies the link metadata.

## Shared Models
- `PrimaryNavSection`: Enum that defines the available primary sections.
- `PrimaryNavConfig`: Associates a primary section label with the secondary link set.
- `SecondaryNavLink`: Represents each secondary navigation entry, including the `NavLinkMatch` mode for router matching.

## Integration Notes
1. `MainLayout` owns the navigation configuration and passes data into both components.
2. Both components stay in the `NexaCRM.Pages.Components.Layout` namespace for straightforward usage within the layout.
3. The layout still controls navigation state (`activePrimaryNav`, `activeSecondaryNavTitle`) and navigation events.

## Testing Guidance
- Run `dotnet build NexaCrmSolution.sln` to validate the layout and component code compiles together under .NET 8.

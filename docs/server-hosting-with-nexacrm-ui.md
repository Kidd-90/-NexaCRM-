# Server Hosting with NexaCRM.UI

## Overview
The Blazor Server host (`NexaCRM.WebServer`) now references the `NexaCRM.UI` Razor Class Library directly. This keeps layouts, shared UI components, and static assets consistent with the WebAssembly host while removing the dependency on the legacy `NexaCRM.Pages` package.

## Key Changes
- `NexaCRM.WebServer` depends on `NexaCRM.UI` for layouts, navigation components, and scoped CSS assets.
- Shared JavaScript modules (for example, `actions.js`) are loaded from `NexaCRM.UI` to keep interactivity identical between hosting models.
- The WebAssembly client also sources its base styles from `NexaCRM.UI`, ensuring both hosts render the same visual design.

## Build Notes
- Build the solution with `dotnet build NexaCrmSolution.sln` after installing the .NET 8 SDK.
- Because both hosts target .NET 8 and rely on the same UI library, a successful build guarantees that Razor components remain in sync across server and client projects.

# Device Platform Detection

This document records the client-side device detection and layout switching mechanics introduced for NexaCRM.

## Overview

NexaCRM exposes a shared JavaScript helper located at `src/NexaCRM.UI/wwwroot/js/device.js`. The helper performs:

- User-Agent Client Hints (`navigator.userAgentData`) inspection with a graceful fallback to the legacy `navigator.userAgent` string.
- Platform normalization into `desktop`, `android`, or `ios` identifiers.
- DOM decorations that stamp the `<html>` and `<body>` elements with platform data attributes and toggle `platform-mobile` / `platform-desktop` classes.
- Automatic application of the `.mobile-layout` marker class on the `.page` wrapper when a mobile platform is detected.
- Throttled refresh hooks on `resize` and `orientationchange` events.

The helper surfaces an interop API (`window.deviceInterop`) consumed by Blazor components and services. The API includes:

- `getPlatform()`: returns the normalized platform token.
- `isMobile()`, `isIOS()`, and `isAndroid()` boolean helpers.
- `refreshPlatformState()`: re-runs detection and reapplies DOM markers.

## Blazor Service Integration

The `IDeviceService` abstraction (namespace `NexaCRM.UI.Services.Interfaces`) now provides:

- `Task<DevicePlatform> GetPlatformAsync()`
- `Task<bool> IsMobileAsync()`
- `Task<bool> IsIosAsync()`
- `Task<bool> IsAndroidAsync()`

`DeviceService` uses JS interop to retrieve the platform token and projects it into the `DevicePlatform` enum. Each call safely falls back to `DevicePlatform.Desktop` when interop is unavailable.

## Layout Behaviour

`MainLayout.razor` reads the device platform once after the first render and appends the `mobile-layout` class to the page container for mobile platforms. Detection now includes a short retry loop so that deferred script loading on mobile Safari (particularly during server prerender) does not force the shell into desktop mode. When a signed-in user is on iOS or Android, the layout switches into a dedicated **mobile shell** that renders:

- A floating header with greeting text, page title, notification badge support, and a rounded avatar chip that mirrors the desktop login indicator.
- A body section that keeps the existing page content present (for debugging) while displaying a "모바일 화면 준비중" card so that testers recognise the mobile experience is under construction.
- A bottom navigation footer with high-touch shortcuts (Home, DB, 할 일, 설정) styled after the design reference shared by stakeholders.

Style overrides live in `wwwroot/css/ui/mobile-layout.css`. The stylesheet hides the sidebar, applies the mobile shell gradients, and adds safe-area aware padding for modern devices.

The JavaScript helper and Blazor layout both write to `data-device-platform` / `data-platform` attributes to aid debugging and future CSS hooks.

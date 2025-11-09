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

`MainLayout.razor` reads the device platform once after the first render and appends the `mobile-layout` class to the page container for mobile platforms. Style overrides live in `wwwroot/css/ui/mobile-layout.css`, which simplifies the shell for smaller screens (hiding the sidebar, stacking header content, and repositioning the login indicator).

The JavaScript helper and Blazor layout both write to `data-device-platform` / `data-platform` attributes to aid debugging and future CSS hooks.

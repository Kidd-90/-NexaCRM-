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

`DeviceService` uses JS interop to retrieve the platform token and projects it into the `DevicePlatform` enum. The service retries transient `JSException` / `InvalidOperationException` failures and now surfaces a `DevicePlatformDetectionException` when the JavaScript runtime is not yet ready so the caller can defer layout decisions until a real platform value is available.

## Layout Behaviour

`MainLayout` now acts as an orchestration layer that decides whether to render the `DesktopShell` or `MobileShell` components placed under `src/NexaCRM.UI/Shared/Layouts/`. Both shells receive the computed page state through parameters, which keeps the mobile and desktop markup completely separated for easier maintenance. The layout process works as follows:

1. During the first render the layout module is initialised and the device platform is resolved through `IDeviceService`.
2. Navigation context (current title, parent information, header icon, and unread notification count) is calculated inside the `MainLayout` code-behind.
3. When the visitor is authenticated and using an Android/iOS device the `MobileShell` component renders the mobile placeholder UI. Desktop or unauthenticated visitors stay on the `DesktopShell` component, which can still opt into the `mobile-layout` class for login views on mobile.

The mobile shell renders:

- A floating header with greeting text, page title, notification badge support, and a rounded avatar chip that mirrors the desktop login indicator.
- A body section that keeps the existing page content present (for debugging) while displaying a "모바일 화면 준비중" card so that testers recognise the mobile experience is under construction.
- A bottom navigation footer with high-touch shortcuts (Home, DB, 할 일, 설정) styled after the design reference shared by stakeholders.

Style overrides live in `wwwroot/css/ui/mobile-layout.css`. The stylesheet hides the sidebar, applies the mobile shell gradients, and adds safe-area aware padding for modern devices.

The JavaScript helper and Blazor layout both write to `data-device-platform` / `data-platform` attributes to aid debugging and future CSS hooks.

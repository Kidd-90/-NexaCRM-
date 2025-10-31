# bUnit Component Testing Guide

## Overview
- **Framework**: [bUnit](https://bunit.dev) provides a lightweight test renderer for Blazor components.
- **Primary Use Case**: Validate UI hierarchy, accessibility attributes, and interaction handlers without spinning up a full browser.
- **Project Location**: `tests/NexaCRM.UI.Tests`

## Getting Started
1. Restore packages: `dotnet restore tests/NexaCRM.UI.Tests/NexaCRM.UI.Tests.csproj`
2. Run the tests: `dotnet test tests/NexaCRM.UI.Tests/NexaCRM.UI.Tests.csproj --configuration Release`
3. Inspect failures directly in the console output—bUnit prints a diff between expected and actual markup.

## Test Authoring Conventions
- **Namespace Pattern**: `NexaCRM.UI.Tests.Components.<Category>` mirrors the component folder structure under `src/NexaCRM.UI`.
- **Base Class**: Inherit from `Bunit.TestContext` to access the in-memory renderer.
- **Parameter Binding**: Use the fluent `.Add(...)` syntax to set `RenderFragment` slots and strongly typed parameters.
- **Assertions**: Prefer semantic checks (`Find`, `MarkupMatches`, attribute reads) over brittle string comparisons.
- **Accessibility**: Every component test should cover ARIA roles, labels, or keyboard-interactive affordances when applicable.

## Current Coverage Snapshot (March 2025)
- `Banner` notifications: ✅ Title rendering, description fallback, dismiss callback, and ARIA live-region behaviour covered in `BannerTests`.
- Future components: Align new shared UI primitives (e.g., density toggles, card surfaces) with matching test suites before shipping.

## Troubleshooting
- Missing `dotnet` SDK will block the commands above. Ensure .NET 8 SDK is installed locally or run the commands via the CI pipeline.
- If a component depends on JS interop, register stubs on the `Services` collection exposed by `TestContext` before rendering.
- Randomized IDs (e.g., GUID-based element IDs) should be accessed through CSS selectors or `IElement` references instead of asserting the literal value.

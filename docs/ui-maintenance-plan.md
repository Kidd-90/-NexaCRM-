# UI Maintenance Plan

## Latest Cleanup Notes (March 2025)
- Removed unused `.grid`, `.card-title`, and `.card-text` selectors from `Pages/BizManagementPage.razor.css` to keep scoped styles aligned with the rendered markup and avoid accidental overrides.
- Enabled wrapping text within the Biz Management preview banner and relaxed its line height to prevent clipping on narrow layouts.
- Normalized the login container width handling by switching to `min(100%, 100vw)` so Blazor pages avoid unexpected horizontal scrollbars while still honoring full-width background treatments.
- Introduced shared radius and shadow tokens in `wwwroot/css/ui/foundations.css`, updated utility classes, and refactored the Biz Management and Login scoped styles to consume the new surface scale.
- Implemented the reusable `Banner` component in `Components/Notifications`, applied scoped styling, and replaced the Biz Management preview banner to validate the hierarchy.

## Recommended Workflow for UI Iterations
1. **Audit Scoped CSS**: For each Razor page being touched, compare the markup classes with the corresponding `.razor.css` file and remove or rename anything no longer in use.
2. **Promote Shared Patterns**: When a selector is reused across multiple pages, migrate the rule into `wwwroot/css/ui/patterns.css` and import it via `ui/index.css` for consistency.
3. **Validate Layout Responsiveness**: Use browser dev tools at 360px, 768px, and 1280px widths to confirm grid/flex ordering and that `min()`/`clamp()` token usage keeps components within the layout grid.
4. **Accessibility Pass**: Check focus outlines, color contrast (WCAG AA), and keyboard navigation after every change. Prefer tokenized colors from `app.css` to keep contrast predictable.
5. **Regression Build**: Run `dotnet build NexaCrmSolution.sln --configuration Release` to ensure Razor scoped CSS changes compile before shipping.

## Design Enhancements to Prioritize
- **Dashboard Density Controls**: Introduce a compact/comfortable toggle for table-heavy pages (use CSS custom properties to swap padding/margin scales).
- **Banner Hierarchy**: Convert informational banners (e.g., preview banner) into a reusable component with leading icon, title, and helper text slots for better content scanning.
- **Form Input Consistency**: Align input heights by reusing the login page’s `--touch-target-min` token in other modules so pointer targets meet 44px minimums.
- **Dark Theme Expansion**: Mirror the new login width logic in other entry points and extend the existing `[data-theme="dark"]` overrides for page-level banners and filters.
- **Micro-interactions**: Apply `prefers-reduced-motion` safe transitions (opacity/translate) when introducing hover states or tab switches on admin dashboards.
- **Surface Treatment Harmonization**: Standardize border radii, shadows, and divider usage so flat cards and rounded panels read as a cohesive system instead of competing motifs.

### Surface Treatment Harmonization Roadmap
- **Codify Radius Tokens**: ✅ Completed. `wwwroot/css/ui/foundations.css` now exports `--radius-none`, `--radius-sm`, `--radius-md`, `--radius-lg`, and `--radius-pill`, and the first wave of scoped CSS refactors replaced hard-coded values with the shared tokens.
- **Differentiate by Component Intent**: Use `--radius-none` for list rows, tables, and navigation rails where crisp separation supports density; reserve `--radius-sm` for inline inputs/buttons; apply `--radius-lg` only to modal shells or hero cards that need higher emphasis.
- **Align Shadow + Border Logic**: Pair flat elements (`--radius-none`) with `border-bottom` or subtle dividers, and rounded elements with `box-shadow` tokens (`--shadow-soft`, `--shadow-deep`) so each surface depth reads consistently.
- **Document Overrides**: Extend the component README snippets (e.g., `Components/Cards/README.md` if present) with “surface” tables showing default radius and how to opt into alternatives using CSS custom properties.
- **QA Checklist Addition**: Update UI review scripts to flag any new component that mixes `border-radius: 0` and `border-radius: 16px` within the same cluster without a documented rationale.
- **Dark Theme Parity**: Mirror the same radius tokens inside `[data-theme="dark"]` so neutral and dark palettes share identical silhouettes; ensure shadows degrade to `box-shadow: none` with increased border opacity for dark surfaces where glow would be distracting.

## Technology Reference
- **Blazor Scoped CSS** (`*.razor.css`) keeps page-level rules encapsulated—leverage it for one-off tweaks, and ensure deletions do not affect other pages.
- **Modern CSS Functions** such as `min()` and `clamp()` are used throughout `NexaCRM.UI` to maintain responsive sizing. Prefer these over hard-coded pixel values when adjusting layouts.
- **Design Tokens** defined in `wwwroot/css/app.css` (font stacks, spacing, colors) should be the source of truth for future UI additions.

## Banner Hierarchy Componentization Plan

### Objectives
- Deliver a reusable banner module that supports informational, warning, and success contexts without duplicating CSS across Razor pages.
- Provide predictable hierarchy via icon, title, description, and action areas so copywriters can compose messages rapidly.
- Ensure the component meets accessibility targets (keyboard focus management, ARIA semantics, and color contrast) for both light and dark themes.

### Component Structure
| Slot | Description | Required |
| --- | --- | --- |
| `LeadingVisual` | 24×24 icon (SVG) aligned to the start of the banner. Defaults to info glyph when not supplied. | No |
| `Title` | One-line heading styled with `--font-weight-semibold`. Use `id` to associate with `aria-labelledby`. | Yes |
| `Description` | Supports multi-line rich text (links allowed). Should wrap and respect `--line-height-base`. | No |
| `Actions` | Inline buttons or links for dismiss/primary CTA. Accepts up to two actions. | No |

### Styling Guidelines
- Base component lives in `Components/Notifications/Banner.razor` with scoped stylesheet `Banner.razor.css` using existing spacing tokens (`--space-3`, `--space-4`).
- Expose CSS custom properties for border, background, and icon colors so page-level overrides can theme the banner without modifying component CSS.
- Implement `data-variant="info|warning|success|danger"` attribute to drive color mapping through tokenized palette (reference `app.css` color variables).
- Use `display: grid` with `grid-template-columns: auto 1fr auto` on ≥768px viewports and collapse to stacked flow on narrow viewports.
- Respect dark mode by reading `[data-theme="dark"]` on the nearest ancestor and switching to dark tokens through the custom properties.

### Accessibility Checklist
- Wrap the component in a `<section role="status">` by default; allow opting into `role="alert"` for high-priority warnings.
- Link `aria-labelledby` to the title slot and `aria-describedby` to the description when provided.
- Ensure focus outlines remain visible for action buttons and provide an optional close button that advertises `aria-label` text.
- Validate color contrast for each variant (target ≥4.5:1 for text/background combinations in both themes).

### Implementation Steps
1. ✅ **Scaffold Component**: Generated the Razor component with named `RenderFragment` slots, variant enum, dismiss support, and accessibility defaults.
2. ✅ **Author Scoped Styles**: Added `Banner.razor.css` with responsive grid layout, token usage, and variant-driven color custom properties.
3. **Add Storybook Example (Optional)**: If UI documentation exists, add banner stories demonstrating each variant and slot combination.
4. ✅ **Integrate First Consumer**: Replaced the Biz Management preview banner markup with the new component to validate alignment and responsive behavior.
5. **Propagate Across Pages**: Identify other ad-hoc banners (`rg "banner" src/NexaCRM.UI/Pages`) and migrate them incrementally, removing redundant CSS.
6. **Write Regression Tests**: Add bUnit snapshot tests covering variant rendering and dismiss events in `tests/NexaCRM.UI.Tests/Components/Notifications`.
7. **QA Checklist**: Verify keyboard navigation, screen reader output (NVDA/JAWS), and mobile viewport wrapping on at least two browsers.

### Dependencies & Risks
- Requires confirmation of icon assets (Font Awesome vs. in-house SVG) for consistent sizing.
- bUnit tests depend on existing component test project; set up fixtures if not already present.
- Dark theme tokens must be validated—missing tokens could delay rollout; coordinate with design to finalize palette.

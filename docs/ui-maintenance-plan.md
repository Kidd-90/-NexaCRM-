# UI Maintenance Plan

## Latest Cleanup Notes (March 2025)
- Removed unused `.grid`, `.card-title`, and `.card-text` selectors from `Pages/BizManagementPage.razor.css` to keep scoped styles aligned with the rendered markup and avoid accidental overrides.
- Enabled wrapping text within the Biz Management preview banner and relaxed its line height to prevent clipping on narrow layouts.
- Normalized the login container width handling by switching to `min(100%, 100vw)` so Blazor pages avoid unexpected horizontal scrollbars while still honoring full-width background treatments.

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

## Technology Reference
- **Blazor Scoped CSS** (`*.razor.css`) keeps page-level rules encapsulated—leverage it for one-off tweaks, and ensure deletions do not affect other pages.
- **Modern CSS Functions** such as `min()` and `clamp()` are used throughout `NexaCRM.UI` to maintain responsive sizing. Prefer these over hard-coded pixel values when adjusting layouts.
- **Design Tokens** defined in `wwwroot/css/app.css` (font stacks, spacing, colors) should be the source of truth for future UI additions.

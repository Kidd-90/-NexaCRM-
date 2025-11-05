# UI Maintenance Plan

## Latest Cleanup Notes (March 2025)
- Removed unused `.grid`, `.card-title`, and `.card-text` selectors from `Pages/BizManagementPage.razor.css` to keep scoped styles aligned with the rendered markup and avoid accidental overrides.
- Enabled wrapping text within the Biz Management preview banner and relaxed its line height to prevent clipping on narrow layouts.
- Normalized the login container width handling by switching to `min(100%, 100vw)` so Blazor pages avoid unexpected horizontal scrollbars while still honoring full-width background treatments.
- Introduced shared radius and shadow tokens in `wwwroot/css/ui/foundations.css`, updated utility classes, and refactored the Biz Management and Login scoped styles to consume the new surface scale.
- Implemented the reusable `Banner` component in `Components/Notifications`, applied scoped styling, and replaced the Biz Management preview banner to validate the hierarchy.
- Propagated the shared banner and surface tokens to `Pages/DbAdvancedManagementPage`, ensuring filters, rule configs, and modal shells align with the radius/shadow scale.
- Authored bUnit regression tests for the shared `Banner` component so accessibility attributes and dismiss callbacks remain stable during future refactors.
- Standardized interactive control heights via `wwwroot/css/ui/forms.css`, ensuring `form-control`, `form-select`, and `btn` classes all respect the 44px touch target token.
- Mirrored the full-width clamp strategy from the login experience into the authenticated layout shell and hardened dark theme surface tokens for Biz/DB management banners, filters, modals, and cards so night mode no longer produces glowing artifacts or stray scrollbars.
- Published `wwwroot/css/ui/motion.css` with shared motion tokens, wired hover/tab micro-interactions into the Biz and Advanced DB dashboards, ensured all transitions disable gracefully under `prefers-reduced-motion`, and documented usage in `docs/micro-interactions.md`.
- Refined `Pages/MainDashboard` KPI 카드와 차트에 공통 토큰, 현황/목표 배지, 다국어 리소스를 적용해 접근 가능한 상태 안내·범례 토글·툴팁·스켈레톤을 제공했습니다.【F:src/NexaCRM.UI/Pages/MainDashboard.razor†L20-L216】【F:src/NexaCRM.UI/Pages/MainDashboard.razor.css†L48-L262】【F:src/NexaCRM.UI/Resources/Pages/MainDashboard.en-US.resx†L25-L89】
- Converted `Shared/ResponsivePage` into a 12컬럼 격자 래퍼와 컬럼 스팬 토큰을 제공하는 유틸리티로 확장하고, 토큰 재사용을 위해 `wwwroot/css/ui/foundations.css`와 `utilities.css`를 갱신했습니다.【F:src/NexaCRM.UI/Shared/ResponsivePage.razor†L1-L27】【F:src/NexaCRM.UI/Shared/ResponsivePage.razor.css†L1-L88】【F:src/NexaCRM.UI/wwwroot/css/ui/foundations.css†L6-L40】【F:src/NexaCRM.UI/wwwroot/css/ui/utilities.css†L9-L112】
- 재구성한 `NavigationTail`에 태블릿 헤더 토글과 뷰포트별 로컬 스토리지 키를 도입하고, `QuickActionsComponent`를 카드 레이아웃/ARIA 리스트 패턴으로 승격해 접근성과 반응형 행동을 맞췄습니다.【F:src/NexaCRM.UI/Shared/NavigationTail.razor†L1-L663】【F:src/NexaCRM.UI/Shared/NavigationTail.razor.css†L1-L446】【F:src/NexaCRM.UI/Components/UI/QuickActionsComponent.razor†L1-L148】【F:src/NexaCRM.UI/Components/UI/QuickActionsComponent.razor.css†L1-L154】
- `Pages/ReportsPage`를 2컬럼 편집 레이아웃과 인라인 상태 피드백, 접근 가능한 미리보기 테이블로 리팩터링하고 다국어 리소스를 확장했습니다.【F:src/NexaCRM.UI/Pages/ReportsPage.razor†L1-L189】【F:src/NexaCRM.UI/Pages/ReportsPage.razor.css†L1-L171】【F:src/NexaCRM.UI/Resources/Pages/ReportsPage.en-US.resx†L21-L86】【F:src/NexaCRM.UI/Resources/Pages/ReportsPage.ko-KR.resx†L21-L86】

## Recommended Workflow for UI Iterations
1. **Audit Scoped CSS**: For each Razor page being touched, compare the markup classes with the corresponding `.razor.css` file and remove or rename anything no longer in use.
2. **Promote Shared Patterns**: When a selector is reused across multiple pages, migrate the rule into `wwwroot/css/ui/patterns.css` and import it via `ui/index.css` for consistency.
3. **Validate Layout Responsiveness**: Use browser dev tools at 360px, 768px, and 1280px widths to confirm grid/flex ordering and that `min()`/`clamp()` token usage keeps components within the layout grid.
4. **Accessibility Pass**: Check focus outlines, color contrast (WCAG AA), and keyboard navigation after every change. Prefer tokenized colors from `app.css` to keep contrast predictable.
5. **Regression Build**: Run `dotnet build NexaCrmSolution.sln --configuration Release` to ensure Razor scoped CSS changes compile before shipping.

## Design Enhancements to Prioritize
- ✅ **Dashboard Density Controls**: Added the `DensityToggle` component, density token sheet, and Biz/DB management integration so operators can switch between 여유/컴팩트 모드 without duplicating scoped CSS.
- **Banner Hierarchy**: Convert informational banners (e.g., preview banner) into a reusable component with leading icon, title, and helper text slots for better content scanning.
- ✅ **Form Input Consistency**: Rolled the login page’s `--touch-target-min` token into shared form helpers so controls across dashboards meet the 44px minimum touch target.
- ✅ **Dark Theme Expansion**: Propagated the login width clamp to the desktop shell and refreshed `[data-theme="dark"]` surface overrides for banners, filters, and modals across Biz and Advanced DB management.
- ✅ **Micro-interactions**: Added motion tokens/utilities and applied them to dashboard toggles, tabs, and action buttons while respecting `prefers-reduced-motion` so hover states feel responsive without overwhelming sensitive users.
- **Surface Treatment Harmonization**: Standardize border radii, shadows, and divider usage so flat cards and rounded panels read as a cohesive system instead of competing motifs.

### Surface Treatment Harmonization Roadmap
- **Codify Radius Tokens**: ✅ Completed. `wwwroot/css/ui/foundations.css` now exports `--radius-none`, `--radius-sm`, `--radius-md`, `--radius-lg`, and `--radius-pill`, and the first wave of scoped CSS refactors replaced hard-coded values with the shared tokens.
- **Differentiate by Component Intent**: Use `--radius-none` for list rows, tables, and navigation rails where crisp separation supports density; reserve `--radius-sm` for inline inputs/buttons; apply `--radius-lg` only to modal shells or hero cards that need higher emphasis.
- **Align Shadow + Border Logic**: Pair flat elements (`--radius-none`) with `border-bottom` or subtle dividers, and rounded elements with `box-shadow` tokens (`--shadow-soft`, `--shadow-deep`) so each surface depth reads consistently.
- **Document Overrides**: Extend the component README snippets (e.g., `Components/Cards/README.md` if present) with “surface” tables showing default radius and how to opt into alternatives using CSS custom properties.
- **QA Checklist Addition**: Update UI review scripts to flag any new component that mixes `border-radius: 0` and `border-radius: 16px` within the same cluster without a documented rationale.
- **Dark Theme Parity**: Mirror the same radius tokens inside `[data-theme="dark"]` so neutral and dark palettes share identical silhouettes; ensure shadows degrade to `box-shadow: none` with increased border opacity for dark surfaces where glow would be distracting.

### Form Input Consistency Rollout
- **Tokenize Control Heights**: ✅ Added `--ui-control-height` scale inside `wwwroot/css/ui/forms.css` mapped to `--touch-target-min` to guarantee 44px minimums.
- **Normalize Base Controls**: ✅ Applied the shared scale to `.form-control`, `.form-select`, and `.btn` classes (including size variants) so filters, modals, and toolbars align without manual overrides.
- **Page Pass**: ✅ Updated Biz/DB management filters to inherit the shared control styling and removed ad-hoc height declarations.
- **QA Checklist**: Verify all newly added inputs and actions render with a minimum of 44px height on 360px/768px/1280px breakpoints; regress `prefers-reduced-motion` focus outlines to confirm they remain within the touch bounds.

## Technology Reference
- **Blazor Scoped CSS** (`*.razor.css`) keeps page-level rules encapsulated—leverage it for one-off tweaks, and ensure deletions do not affect other pages.
- **Modern CSS Functions** such as `min()` and `clamp()` are used throughout `NexaCRM.UI` to maintain responsive sizing. Prefer these over hard-coded pixel values when adjusting layouts.
- **Design Tokens** defined in `wwwroot/css/app.css` (font stacks, spacing, colors) should be the source of truth for future UI additions.
- **Component Testing** guidance lives in `docs/bunit-component-testing.md`; follow it when introducing new shared UI primitives or updating existing suites.

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
5. ✅ **Propagate Across Pages**: Migrated the ad-hoc banner on `DbAdvancedManagementPage` to the shared component and removed duplicate styling; continue auditing newly added pages during feature work.
6. ✅ **Write Regression Tests**: Added bUnit interaction and accessibility tests in `tests/NexaCRM.UI.Tests/Components/Notifications` to cover variants, live-region behaviour, and dismiss callbacks.
7. **QA Checklist**: Verify keyboard navigation, screen reader output (NVDA/JAWS), and mobile viewport wrapping on at least two browsers.

## Density Control Rollout Checklist
- ✅ **Component Foundation**: Introduced `DensityMode` enum, `DensityPreference` helpers, and the slot-based `DensityToggle` control with ARIA semantics.
- ✅ **Tokenization**: Published `wwwroot/css/ui/density.css` to expose shared gap and table padding variables consumed by dashboard pages.
- ✅ **Page Integration**: Wired Biz/DB management containers to store the density mode in local storage, update toolbar layouts, and adjust table/form spacing via CSS variables.
- **QA Tasks**: Capture screenshots for compact mode across 768px/1280px breakpoints and run cross-browser reduced-motion checks.

### Dependencies & Risks
- Requires confirmation of icon assets (Font Awesome vs. in-house SVG) for consistent sizing.
- bUnit tests depend on existing component test project; set up fixtures if not already present.
- Dark theme tokens must be validated—missing tokens could delay rollout; coordinate with design to finalize palette.

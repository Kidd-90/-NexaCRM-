# Surface Treatment Consistency Playbook

## Purpose
- Resolve the visual clash between flat (sharp-cornered) layouts and rounded panels across NexaCRM UI.
- Provide actionable guardrails so future components adopt a single, intentional surface language.
- Ensure both light and dark themes apply the same silhouette hierarchy with token-based overrides.

## Audit Highlights
1. **Navigation Rail vs. Cards**
   - Rail modules render with `border-radius: 0` and divider-based separation, while dashboard cards use `border-radius: 16px` plus deep shadows.
   - Recommendation: retain flat navigation for clarity, but drop card radius to `--radius-sm` and tone down shadows when cards appear adjacent to the rail.
2. **List/Table Rows**
   - Many row templates mix flat background lines with occasional pill-shaped status badges.
   - Recommendation: keep rows flat, but align badge pill radius to the new token scale and document when pills may be swapped for tags or chips.
3. **Authentication Shells**
   - Login card uses glassmorphism with `border-radius: 24px`, while recovery dialogs use flat edges.
   - Recommendation: pin auth surfaces to `--radius-lg` and update recovery dialogs accordingly; ensure mobile breakpoints fall back to `--radius-sm` to maximize viewport width.

## Standard Radius Scale
| Token | Value | Usage |
| --- | --- | --- |
| `--radius-none` | `0px` | Navigation rails, tables, list items, separators. |
| `--radius-sm` | `8px` | Buttons, inputs, compact cards, dropdown menus. |
| `--radius-lg` | `18px` | Dialog shells, hero cards, authentication containers. |

> Declare the tokens inside `wwwroot/css/ui/foundations.css` and replace existing literal values via `rg "border-radius" src/NexaCRM.UI`.

## Shadow + Divider Guidelines
- Flat surfaces (`--radius-none`) should rely on `border-bottom` or `--divider-color` to express grouping.
- Rounded surfaces pair with `--shadow-soft` (default) or `--shadow-deep` (spotlight moments). Avoid mixing shadows on flat cards.
- In dark mode, prefer `box-shadow: none` plus 1px translucent borders (`rgba(var(--neutral-100-rgb), 0.35)`) to prevent halo artifacts.

## Implementation Checklist
1. **Tokenize**: Add radius tokens to `foundations.css`, export via `:root`, and include `[data-theme="dark"]` overrides if different values are required.
2. **Refactor Components**: Update `Pages/*.razor.css` and `Components/**/*.razor.css` to consume the tokens. Focus on Biz Management cards, login shell, modal dialogs, and status banners first.
3. **Utility Classes**: Add `.surface-flat`, `.surface-rounded`, `.surface-hero` helpers in `wwwroot/css/ui/utilities.css` mapping to the token scale for rapid prototyping.
4. **Design QA Script**: Extend the Figma or Storybook checklist to confirm consistent corner radii across breakpoints; flag any combination of flat and heavily rounded elements within the same layout region.
5. **Documentation**: Update each component README with a “Surface Treatment” section referencing the helper classes and when overrides are acceptable.
6. **Regression Testing**: After refactoring, run `dotnet build NexaCrmSolution.sln --configuration Release` and targeted visual regression/Playwright checks to ensure no layout breakage.

## Risk Mitigation
- Maintain a migration log inside `docs/ui-maintenance-plan.md` noting which components were tokenized and any exceptions.
- Coordinate with design to phase out legacy gradients that depend on heavy shadows once the new surface hierarchy ships.
- Provide fallback CSS for browsers that do not support CSS custom properties by supplying legacy `border-radius` declarations immediately after the token usage.

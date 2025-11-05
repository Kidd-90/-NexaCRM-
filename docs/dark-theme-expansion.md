# Dark Theme Expansion Guide

## Overview
- 목적: 로그인 경험에서 검증된 폭 처리와 표면 토큰을 메인 레이아웃과 관리형 페이지(Biz, Advanced DB)에 확장해 다크 테마에서도 일관된 브랜드 경험을 제공합니다.
- 범위: `src/NexaCRM.UI/Shared/MainLayout.razor.css`, `Pages/BizManagementPage.razor.css`, `Pages/DbAdvancedManagementPage.razor.css`, `Components/Notifications/Banner` 커스텀 프로퍼티.

## Layout Clamp Rollout
1. `.desktop-shell`에 `width: min(100%, 100vw);`를 적용해 고해상도 모니터에서도 배경 그라디언트가 가로 스크롤을 만들지 않도록 했습니다.
2. 추가적인 페이지가 데스크톱 쉘을 사용할 경우, 별도의 폭 조정 없이 동일한 규칙을 상속합니다.
3. 레이아웃 확장을 수정할 때는 `overflow-x: hidden;`과의 상호작용을 고려하고, 테스트 해상도(1280px, 1440px, 1920px)에서 화면 플리커가 없는지 확인하세요.

## Surface Tokens for Management Pages
| 영역 | 라이트 모드 | 다크 모드 |
| --- | --- | --- |
| 필터/카드 | `background: var(--surface-color)` + `border: var(--border-color)` | 선형 그라디언트(`rgba(37,42,74,0.96)` → `rgba(26,32,58,0.92)`), 보더 `rgba(148,163,184,0.24)` |
| 배너 | `.advanced-db__banner`에서 `--banner-*` 프로퍼티 재정의 | `[data-theme="dark"] .advanced-db__banner`에서 밝은 파랑(#8db4ff) 아이콘과 중성 텍스트 적용 |
| 모달 | `background: var(--surface-color)` + `box-shadow: var(--shadow-deep-layer)` | `[data-theme="dark"] .modal-panel`에 진한 그라디언트와 완화된 보더 적용 |

### Implementation Checklist
- [x] 새 페이지에 배너를 추가할 경우, `.page-scope__banner` 클래스를 만들고 기본/다크 테마에서 사용할 `--banner-*` 값을 정의합니다.
- [x] 필터나 카드 컨테이너가 있다면 `var(--surface-color)`/`var(--surface-muted)` 토큰을 사용하고, `[data-theme="dark"]`에서 별도의 박스 섀도우 값을 지정합니다.
- [x] 모달 헤더/푸터 보더는 `var(--border-color)`를 사용해 테마별 농도를 자동으로 조정합니다.
- [x] QA 시 NVDA/JAWS로 배너 메시지가 `role="status"` 또는 `role="alert"`로 노출되는지 확인하고, 다크 모드 전환 시 대비비율(4.5:1 이상)을 측정합니다.

### QA Checklist (2025-03 업데이트)
1. **수동 확인**
   - `https://localhost:7065/main-dashboard` 에서 테마 토글을 두 차례 수행하고, 카드 대비가 4.5:1 이상인지 `Accessible Color Picker`로 측정합니다.
   - Biz/Advanced DB 모달·필터·배너에서 `--radius-*`와 `--shadow-*` 토큰이 올바르게 치환되었는지 DevTools `Computed` 탭으로 검증합니다.
2. **스크린리더 스팟 체크**
   - NVDA로 배너 영역을 탐색해 `role="status"` 아리아가 읽히는지 확인합니다.
3. **Playwright 회귀**
   - `npx playwright test tests/e2e/dark-theme.spec.js --headed` 를 실행해 다크 테마 토글과 토큰 유지 여부를 캡처합니다.
4. **Storybook/디자인 토큰 스냅샷**
   - Storybook 디자인 토큰 스토리에서 다크/라이트 스냅샷을 비교하고, 변경 사항은 PR 주석 또는 테스트 아티팩트로 첨부합니다.

## Testing Guidance
- `dotnet build NexaCrmSolution.sln --configuration Release`
- 페이지 수준 수동 테스트: 다크/라이트 토글 후 Biz Management와 Advanced DB 페이지를 Chrome DevTools Device Toolbar(360px, 768px, 1280px)에서 확인합니다.
- 자동화 확대 예정: 향후 bUnit 컴포넌트 테스트에서 다크 테마 슬라이스를 Snapshot으로 검증하는 시나리오를 추가할 수 있습니다.

## Known Follow-ups
- ✅ Storybook 및 Playwright 비주얼 리그레션에 다크 테마 시나리오를 등록했습니다 (`tests/e2e/dark-theme.spec.js`).
- KPI 대시보드 카드(`MainDashboard`)에도 동일한 토큰 맵핑을 적용해 전체 제품군에서 그림자/보더 스케일을 통일.

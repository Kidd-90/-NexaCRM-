# NexaCRM Frontend Style Sharing

## Overview
- 공통 디자인 토큰과 스타일 자산은 **Razor Class Library**인 `NexaCRM.UI` 프로젝트에서 관리합니다.
- `NexaCRM.WebClient`와 `NexaCRM.WebServer` 모두 `NexaCRM.UI`를 참조하고, `_content/NexaCRM.UI/...` 정적 자산 경로를 통해 동일한 CSS를 로드합니다.

### Desktop-first shell
- 2025년 2월 이후 UI 셸은 데스크톱 전용 플렉스 레이아웃으로 단순화되었습니다.
- 모바일 전용 규칙과 오버레이는 제거되었으며, `app.css`가 헤더·내비게이션·콘텐츠 패널을 모두 포함합니다.
- 공통으로 반복되는 반응형 패턴(예: 대시보드 사이드바, 카드 그리드)은 `ui/patterns.css`에서 중앙 집중 관리합니다.
- 개별 페이지에서만 필요한 미세 조정은 해당 페이지의 `.razor.css` 파일(Blazor scoped CSS)에만 남겨 둡니다.

## Included Assets
- `wwwroot/css/app.css`: 데스크톱 테마, 전역 레이아웃, 상단 헤더, 사이드바, 카드 컴포넌트 스타일 정의.
- `wwwroot/css/ui/index.css`: 토큰(`foundations.css`), 유틸리티(`utilities.css`), 밀도 토큰(`density.css`), 폼 컨트롤(`forms.css`), 컴포넌트(`components.css`), 레이아웃(`layout.css`), 패턴(`patterns.css`) 계층을 순서대로 불러오는 엔트리 포인트.
- `wwwroot/css/ui/motion.css`: 모션 지속시간/이징 토큰과 `motion-fade-in`, `motion-pressable`, `motion-stagger` 유틸리티를 제공해 공통 마이크로 인터랙션을 표준화합니다.
- `wwwroot/js/*.js`: 인증, 내비게이션, 테마, 디바이스 감지 등 공통 상호작용 로직 모음.
- `https://tweakcn.com/live-preview.min.js`: TweakCN 라이브 프리뷰 스니펫. 디자인 팀이 [TweakCN 테마 편집기](https://tweakcn.com/editor/theme?p=custom)와 실시간으로 스타일을 연동할 때 사용합니다.

## Typography Tokens
- `app.css` 상단에서 `Pretendard Variable` 가변 글꼴을 `@font-face`로 선언하고, `--font-family-sans`, `--font-family-heading`, `--font-family-mono` 등
  전역 타이포그래피 토큰을 제공합니다.
- `--font-size-xs`부터 `--font-size-3xl`까지의 폰트 크기 토큰과 `--body-line-height`, `--heading-line-height-tight` 변수를 통해 페이지마다 일관된 타이포그래피 스케일을 적용할 수 있습니다.
- 새로운 컴포넌트를 추가할 때는 `var(--font-family-sans)` 또는 `var(--font-family-heading)`을 사용하고, 필요 시 `--font-weight-*` 토큰으로 굵기를 설정하세요.

## White Theme Neutral Palette
- `app.css`는 화이트 테마 전용 뉴트럴 팔레트를 커스텀 프로퍼티로 노출합니다. 아래 표는 각 색상과 추천 사용처입니다.

| 구분 | HEX 코드 | 활용 예시 | 효과 응용 |
| --- | --- | --- | --- |
| 화이트 | `#FFFFFF` | 기본 텍스트, 대비 | 글래스모피즘 배경, Soft Glow |
| 라이트 그레이 | `#E0E0E0` | 배경, 섹션 구분 | 그라디언트 시작점, 얇은 보더 |
| 미드 그레이 | `#B0B0B0` | 서브텍스트, 보조 UI | Overlay 톤, 중간 단계 그라디언트 |
| 다크 그레이 | `#4A4A4A` | 사이드바, 아이콘 | 그라디언트 중간톤, Shadow 배경 |
| 딥 그레이 | `#2E2E2E` | 강조 배경, 패널 | 딥 섀도우, 버튼 Hover 배경 |
| 블랙 | `#000000` | 텍스트, 아이콘 | 딥 그라디언트 끝점, 강한 그림자 |

- 주요 그라디언트와 그림자, 오버레이 토큰은 다음과 같이 사용할 수 있습니다.
  - `--gradient-light`: `linear-gradient(180deg, #FFFFFF 0%, #E0E0E0 100%)`
  - `--gradient-medium`: `linear-gradient(145deg, #B0B0B0 0%, #4A4A4A 100%)`
  - `--gradient-deep`: `linear-gradient(160deg, #2E2E2E 0%, #000000 100%)`
  - `--shadow-soft-layer`: `0px 2px 6px rgba(15, 23, 42, 0.12)`
  - `--shadow-deep-layer`: `0px 4px 16px rgba(15, 23, 42, 0.22)`
  - `--overlay-glass-white`: `rgba(255, 255, 255, 0.1)` (blur와 함께 사용)
  - `--overlay-black`: `rgba(0, 0, 0, 0.5)`
  - `--glow-white`: `0px 0px 8px rgba(255, 255, 255, 0.8)`
  - `--glow-dark`: `0px 0px 6px rgba(0, 0, 0, 0.6)`

- 공통 유틸리티 클래스는 다음을 제공합니다.
  ```html
  <div class="white-theme white-theme-gradient-light white-theme-border">
      <div class="white-theme-surface white-theme-glass">
          <p class="text-glow-white">...</p>
      </div>
  </div>
  ```
  - `white-theme`, `white-theme-surface`, `white-theme-surface-muted`: 배경/타이포 일관성 유지.
  - `white-theme-glass`: 글래스모피즘 효과(blur + 반투명 화이트 오버레이).
  - `white-theme-gradient-*`: 제공된 밝기 단계별 그라디언트 적용.
  - `shadow-soft`, `shadow-deep`: 지정된 깊이의 박스 섀도우 빠른 적용.
  - `text-glow-white`, `text-glow-dark`: 텍스트 글로우 효과 프리셋.

> **Tip**: 카드, 헤더, 패널에 `white-theme-glass shadow-soft` 조합을 사용하면 Glassmorphism 기반의 화이트 테마를 쉽게 조합할 수 있습니다.

## Login Experience Refresh
- `Pages/LoginPage.razor` 컨테이너에 `white-theme`와 `white-theme-gradient-light` 클래스를 부여해 전역 화이트 팔레트의 배경/텍스트 토큰을 그대로 사용합니다.
- `LoginPage.razor.css`에서는 `--surface-color`, `--surface-muted`, `--input-*`, `--button-gradient` 등 화이트 테마용 커스텀 프로퍼티를 재정의하여 글래스모피즘 카드, 입력 필드, CTA 버튼이 뉴트럴 팔레트에 맞춰 렌더링됩니다.
- 다크 테마에서도 동일한 컴포넌트 구조를 유지할 수 있도록 `data-theme="dark"` 범위에서 버튼 그라디언트와 포커스 링, 링크 색상을 재조정했습니다.
- 소셜 로그인 카드와 패스워드 토글과 같은 상호작용 요소는 전역 `--focus-ring` 토큰과 `var(--surface-muted)` 조합을 사용해 접근성과 일관성을 확보했습니다.

## Dark Theme Surface Expansion
- `Shared/MainLayout.razor.css`의 `.desktop-shell`은 이제 `width: min(100%, 100vw);`를 사용해 로그인 페이지에서 검증한 풀블리드 폭 처리 방식을 재사용합니다. 이로써 다크 테마 배경 그라디언트가 데스크톱 앱에서도 가로 스크롤을 유발하지 않습니다.
- `Pages/BizManagementPage.razor.css`와 `Pages/DbAdvancedManagementPage.razor.css`는 배너, 필터, 카드, 모달 표면에 `var(--surface-color)`/`var(--surface-muted)` 토큰을 매핑하고, `[data-theme="dark"]` 범위에서 선형 그라디언트와 보더 농도를 조정해 글로우 현상을 제거했습니다.
- 배너 컴포넌트는 페이지 스코프 클래스(예: `.advanced-db__banner`)에서 `--banner-*` 커스텀 프로퍼티를 재정의하고, 다크 테마 전용 오버라이드로 조직 색상(`--org-primary`)을 밝은 파랑(#8db4ff) 계열로 보정해 야간 모드에서도 정보 계층이 동일하게 인식됩니다.
- 새 표면 토큰은 bUnit 테스트나 Storybook 문서 없이도 재사용할 수 있도록 `docs/dark-theme-expansion.md`에서 상세 가이드를 제공합니다. 새로운 페이지가 다크 테마를 지원하려면 이 문서의 체크리스트를 참고해 표면/테두리/타이포 토큰을 지정하세요.

## Form Control Scale
- `wwwroot/css/ui/forms.css`는 로그인에서 사용하던 `--touch-target-min` 값을 기반으로 `--ui-control-height` 토큰을 노출하며, 모든 `.form-control`, `.form-select`, `.btn` 클래스에 동일한 높이와 패딩을 적용합니다.
- `.btn-sm`와 같은 크기 변형도 최소 44px 높이를 유지하고 가로 패딩만 조정해 터치 타깃 기준을 깨지 않도록 구성했습니다.
- 페이지 전용 필터나 툴바에 별도 높이 값을 지정할 필요가 없어지므로, 새 입력/버튼을 추가할 때는 기본 부트스트랩 클래스를 그대로 사용하고 필요한 경우 보조 유틸리티(`.ui-input-height-lg` 등)만 추가하면 됩니다.
- QA 시 360px/768px/1280px 뷰포트에서 `form-control`과 `btn` 요소가 `--touch-target-min` 이상으로 렌더링되는지 확인하세요.

## Usage Guidance
1. Razor 컴포넌트 또는 레이아웃에서 다음과 같이 정적 자산을 참조합니다.
   ```html
   <HeadContent>
      <link rel="stylesheet" href="_content/NexaCRM.UI/css/app.css" />
      <link rel="stylesheet" href="_content/NexaCRM.UI/css/ui/index.css" />
   </HeadContent>
   ```
2. WebAssembly 호스트(`wwwroot/index.html`)에서도 동일 경로를 사용하면 초기 로딩 스피너까지 일관된 스타일을 유지할 수 있습니다.
3. 공통 스크립트가 필요하면 `<script src="_content/NexaCRM.UI/js/<file-name>.js"></script>` 형태로 포함해 두 애플리케이션에서 동일한 UX 동작을 제공합니다.
4. 새로운 공통 스타일이나 스크립트는 `NexaCRM.UI/wwwroot/` 아래에 추가하고, 두 애플리케이션에서 `_content/NexaCRM.UI/...` 경로로 참조하세요. 반복해서 등장하는 반응형 보조 스타일은 `wwwroot/css/ui/patterns.css`에 배치하고, 페이지 고유 규칙은 해당 `.razor.css` 파일에만 유지합니다.

## Page-scoped CSS Guidelines
- 페이지 전용 `.razor.css` 파일에는 **그 페이지에서만 사용되는 선택자**와 레이아웃 미세 조정만 남깁니다.
- 여러 페이지에서 재사용되는 패턴(대시보드 카드 간격, 사이드바 숨김, 테이블 스크롤 처리 등)은 `ui/patterns.css`로 이동해 재사용성을 높이고 유지보수 범위를 축소합니다.
- 공통 패턴에 맞추기 어렵거나 실험적인 디자인은 `@layer page { ... }` 블록을 활용해 범위를 명확히 하고, 필요 시 `patterns.css`로 승격합니다.
- 전체 뷰포트 폭을 채우는 배경이 필요할 때는 `width: min(100%, 100vw)`처럼 현대 CSS 함수를 활용해 스크롤바로 인한 가로 스크롤을 방지하고, 사용되지 않는 선택자는 즉시 제거해 스코프 누수를 예방하세요.

## Server Host Integration Checklist
- `src/NexaCRM.WebServer/Pages/_Host.cshtml` 파일에서 WebClient와 동일한 `_content/NexaCRM.UI` CSS, JS 번들을 참조해 서버 렌더링과 WebAssembly 클라이언트 간 스타일 일관성을 확보합니다.
- 초기 오류 배너(`#blazor-error-ui`)와 `window.isDarkMode` 헬퍼를 포함해 공통 테마 전환 로직이 양쪽 호스트 페이지에서 동일하게 동작하도록 유지합니다.
- 변경 후에는 `dotnet build NexaCrmSolution.sln --configuration Release` 명령으로 Razor Class Library 참조가 유효한지 확인합니다.

## Change Management
- 공통 CSS를 수정할 때는 `NexaCRM.UI` 프로젝트의 파일만 변경하고, `dotnet build --configuration Release`로 변경 사항이 정상적으로 적용되는지 확인합니다.
- 스타일 변경 이후에는 WebClient와 WebServer 모두에서 UI 회귀 테스트(수동 또는 자동)를 수행하여 디자인 일관성을 검증하세요.

## Component Updates
- `NavigationTail` 패널 인터랙션을 `rail-panel-item` 클래스로 통합하고, 아이콘과 라벨 간 간격을 40px 그리드로 맞춰 데스크톱/태블릿에서 동일한 정렬을 유지합니다.
- 패널 항목에 그라디언트 하이라이트, 왼쪽 엣지 인디케이터, 미세한 슬라이드 모션을 추가하여 그룹 전환 시 진행감과 활성 상태 인지를 개선했습니다.
- `prefers-reduced-motion` 미디어 쿼리를 통해 패널/항목 애니메이션을 자동으로 비활성화해 접근성 요구 사항을 충족합니다.
- 패널이 확장될 때 `railPanelReveal` 키프레임으로 슬라이드 인·스프링 효과를 적용하고, 접힘 상태에는 스케일 축소와 투명도 전환을 함께 적용해 자연스러운 전환을 제공합니다.
- `Components/Notifications/Banner`를 추가해 정보, 경고, 성공, 위험 배너를 슬롯 기반으로 구성하고, `BannerVariant` 열거형과 `data-variant` 속성으로 색상/아이콘을 제어합니다. 페이지 스코프에서는 `Class` 매개변수로 여백만 조정하고, 시각적 처리는 컴포넌트 스코프에서 유지합니다.
- `Components/Controls/DensityToggle`은 `DensityMode` 값에 따라 버튼 그룹을 표시하고, 페이지 루트에 설정된 `data-density` 속성과 `ui/density.css` 토큰을 사용해 테이블/필터 간격과 글꼴 스케일을 전환합니다.
- `ui/motion.css`는 `prefers-reduced-motion` 친화적인 `motion-fade-in`·`motion-pressable`·`motion-stagger` 클래스를 제공해 배너, 토글, 대시보드 탭의 마이크로 인터랙션을 일관되게 구성합니다.

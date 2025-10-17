# CSS Framework Dependency Audit

## Overview
- 본 문서는 NexaCRM UI가 의존하고 있는 CSS 프레임워크(Tailwind CSS, Bootstrap)를 정리해 향후 제거 또는 대체 작업의 범위를 명확히 하기 위해 작성되었습니다.
- 범위에는 Razor Class Library(`NexaCRM.UI`), 공유 페이지 리소스(`NexaCRM.Pages`), 그리고 WebClient 호스트 문서가 포함됩니다.

## Tailwind CSS 의존도
1. **런타임 주입 제거**
   - `NexaCRM.UI`의 메인/로그인 레이아웃은 Tailwind CDN 스크립트를 제거하고, 부트스트랩 기반 번들(`ui/index.css`) 하나만 로드하도록 단순화했습니다. 번들은 `@import`로 기초 토큰(`foundations.css`), 단일 책임 유틸리티(`utilities.css`), 컴포넌트 패턴(`components.css`), 레이아웃 그리드(`layout.css`)를 순서대로 합쳐 Tailwind 시절 마크업을 안정적으로 치환합니다.【F:src/NexaCRM.UI/Shared/MainLayout.razor†L14-L50】【F:src/NexaCRM.UI/Shared/LoginLayout.razor†L3-L35】【F:src/NexaCRM.UI/wwwroot/css/ui/index.css†L1-L8】【F:src/NexaCRM.UI/wwwroot/css/ui/foundations.css†L1-L20】【F:src/NexaCRM.UI/wwwroot/css/ui/utilities.css†L1-L122】【F:src/NexaCRM.UI/wwwroot/css/ui/components.css†L1-L129】【F:src/NexaCRM.UI/wwwroot/css/ui/layout.css†L1-L32】
2. **빌드 파이프라인**
   - `NexaCRM.Pages` 프로젝트는 여전히 Tailwind 4.x CLI를 devDependency로 포함하며, `npm run build:css` 스크립트가 Pages 전용 유틸리티 번들을 생성합니다.【F:src/NexaCRM.Pages/package.json†L1-L22】
   - Tailwind 입력 파일(`wwwroot/css/tailwind.css`)은 기본 유틸리티 import 및 커스텀 컴포넌트(`.naf-button`) 정의를 포함해 Pages UI를 구성합니다.【F:src/NexaCRM.Pages/wwwroot/css/tailwind.css†L1-L13】
3. **보조 스타일 계층**
   - `NexaCRM.UI/wwwroot/css/app.css`는 전역 토큰과 부트스트랩 오버라이드를 제공하고, `ui/utilities.css`는 `ui-min-h-*`, `ui-width-*` 등의 단일 속성 헬퍼를, `ui/components.css`는 카드·툴바·배지 같은 반복 컴포넌트를, `ui/layout.css`는 통계 그리드/패널 배치를 담당합니다.【F:src/NexaCRM.UI/wwwroot/css/app.css†L1-L120】【F:src/NexaCRM.UI/wwwroot/css/ui/utilities.css†L1-L122】【F:src/NexaCRM.UI/wwwroot/css/ui/components.css†L1-L129】【F:src/NexaCRM.UI/wwwroot/css/ui/layout.css†L1-L32】

> **결론**: UI Razor Class Library는 Tailwind CDN 없이 부트스트랩과 정적 유틸리티 번들만으로 동작합니다. Pages 프로젝트는 여전히 Tailwind 빌드 파이프라인을 사용하므로, 전사 통합을 위해서는 추가 전환 계획이 필요합니다.

## Bootstrap 의존도
1. **CDN 스타일시트**
   - 메인 레이아웃과 로그인 레이아웃 모두 Bootstrap 5.3, Open Iconic, Bootstrap Icons를 CDN으로 로드합니다. 헤더 버튼, 알림 배지, 아이콘 렌더링에 Bootstrap 아이콘 클래스를 직접 사용합니다.【F:src/NexaCRM.UI/Shared/MainLayout.razor†L14-L80】【F:src/NexaCRM.UI/Shared/LoginLayout.razor†L3-L48】
2. **문서화 및 기대치**
   - WebClient 문서에서도 현재 스타일 프레임워크로 Bootstrap을 명시하고 있어, 팀 차원의 가이드가 Bootstrap 기반 UX를 전제로 작성되어 있습니다.【F:docs/WEB_CLIENT_DOCUMENTATION.md†L40-L60】

> **결론**: Bootstrap은 주로 아이콘 세트 및 기본 유틸리티(예: spacing, 버튼 클래스)에 사용되고 있으며, CDN 링크를 제거할 경우 `bi bi-*` 아이콘이 모두 사라집니다. 대체 아이콘 시스템 또는 자체 컴포넌트가 준비되기 전에는 유지가 필요합니다.

## 권장 후속 조치
- Pages 레이어에서 Tailwind를 완전히 제거하려면 Razor 컴포넌트의 유틸리티 사용을 조사하고, 동일 기능을 Bootstrap 또는 정적 CSS로 대체할 계획을 수립해야 합니다.
- Bootstrap 의존도를 줄이고자 한다면 `bootstrap-icons`를 대체할 SVG 아이콘 자산을 정의하고, 버튼/그리드 구성에 쓰이는 `.btn`, `.row` 등의 사용 여부를 추가로 조사한 뒤 제거 일정을 수립하세요.

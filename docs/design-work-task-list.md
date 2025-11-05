# NexaCRM 디자인 작업 진행 리스트

## 1. 레이아웃 및 반응형 재정비
- [x] `MainDashboard`와 `ResponsivePage` 전체에 12컬럼 기반 CSS 토큰을 도입해 태블릿·데스크탑 폭을 표준화합니다. `clamp()`를 활용해 768px, 1200px, 1600px 이상 구간을 명확히 나눕니다.【F:docs/design-task-check-2025-04.md†L35-L43】【F:docs/tablet-desktop-ui-enhancement-checklist.md†L16-L44】
- [x] 태블릿 구간에서 퀵 액션 바를 카드화하고, 네비게이션 레일을 접힘 가능한 구성으로 재배치합니다. 네비게이션 상태 저장 로직은 뷰포트별로 분리합니다.【F:docs/design-task-check-2025-04.md†L44-L50】【F:docs/tablet-desktop-ui-enhancement-checklist.md†L28-L41】

## 2. 대시보드 상호작용 고도화
- [x] KPI 카드에 증감 지표, 목표 대비 배지, 실시간 갱신 타임스탬프를 추가하고 다크 테마 토큰과 연동합니다.【F:src/NexaCRM.UI/Pages/MainDashboard.razor†L20-L120】【F:src/NexaCRM.UI/Pages/MainDashboard.razor.css†L48-L154】【F:src/NexaCRM.UI/Resources/Pages/MainDashboard.en-US.resx†L25-L63】
- [x] SVG 차트에 호버/포커스 툴팁, 평균선, 범례 토글, 로딩 스켈레톤 애니메이션을 정의합니다.【F:src/NexaCRM.UI/Pages/MainDashboard.razor†L122-L216】【F:src/NexaCRM.UI/Pages/MainDashboard.razor.css†L156-L262】【F:src/NexaCRM.UI/Resources/Pages/MainDashboard.ko-KR.resx†L45-L85】

## 3. 네비게이션 & 정보 구조 정렬
- [x] `NavigationTail`의 경로 동기화 로직을 유지하면서, 태블릿 이하에서는 헤더 퀵 액션으로 대체되는 패널 상태 다이어그램을 정의합니다.【F:docs/design-task-check-2025-04.md†L47-L50】【F:docs/tablet-desktop-ui-enhancement-checklist.md†L45-L55】
- [x] 전역 검색·알림·설정의 키보드 포커스 순서와 스킵 링크 전략을 문서화하고, 다국어 문자열 길이에 맞춘 버튼 폭 규칙을 추가합니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L46-L55】【F:docs/tablet-desktop-ui-enhancement-checklist.md†L96-L109】

## 4. 표면·토큰 통합
- [x] `foundations.css`에 표준 `--radius-*` 토큰을 선언하고, 주요 컴포넌트 CSS에서 하드코딩된 `border-radius` 값을 토큰으로 교체합니다.【F:docs/surface-treatment-consistency.md†L20-L43】
- [x] `.surface-flat`, `.surface-rounded`, `.surface-hero` 유틸리티 클래스를 정의해 카드·배너·모달에 동일한 표면 언어를 적용합니다.【F:docs/surface-treatment-consistency.md†L44-L59】

## 5. 다크 테마 확장 & QA
- [x] Biz/Advanced DB 페이지의 배너·카드·모달이 다크 토큰을 상속하는지 QA 체크리스트를 작성하고, 대비비율 4.5:1 이상을 유지합니다.【F:docs/dark-theme-expansion.md†L5-L38】
- [x] Storybook 또는 Playwright 시각 회귀 테스트에 다크 테마 시나리오를 등록해 회귀를 감시합니다.【F:docs/dark-theme-expansion.md†L49-L53】

## 6. 데이터 입력 & 테이블 경험 개선
- [x] `ReportsPage` 등 편집 화면을 태블릿 2열, 데스크탑 멀티패널 레이아웃으로 재설계하고 인라인 검증·고정 액션 영역을 추가합니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L78-L93】
- [x] 폼·필터·테이블 구성 요소의 상태(로딩, 성공, 오류) 스타일을 토큰화하고, 저장/취소 흐름을 마이크로 인터랙션과 연결합니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L100-L119】【F:docs/micro-interactions.md†L1-L47】

## 7. 마이크로 인터랙션 & 접근성
- [x] 네비게이션, 토스트, 탭 전환에 0.3~0.4초 이징 커브와 `prefers-reduced-motion` 대응 애니메이션 토큰을 정의합니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L120-L132】【F:docs/micro-interactions.md†L8-L33】
- [x] `ARIA` 속성, 포커스 스타일, 스킵 링크, 라이브 영역 설정을 전역 컴포넌트로 확장하고 QA 스크립트를 준비합니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L133-L149】

## 8. 기술 연계 & 문서화
- [x] 각 작업 완료 후 `docs/ui-maintenance-plan.md`와 관련 컴포넌트 README에 결과를 기록합니다.【F:docs/surface-treatment-consistency.md†L60-L68】【F:docs/ui-maintenance-plan.md†L1-L120】
- [x] 디자인 토큰 변경 시 `dotnet build NexaCrmSolution.sln --configuration Release`를 실행해 빌드 회귀를 즉시 확인합니다.【F:docs/dark-theme-expansion.md†L41-L48】

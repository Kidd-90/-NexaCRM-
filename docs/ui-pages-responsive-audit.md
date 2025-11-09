# UI Pages Responsive Audit (Step 1)

## Scope
- 조사 대상: `src/NexaCRM.UI/Pages`의 Razor 페이지와 연결된 `.razor.css` 파일 전반.
- 목표: 모바일 전용 마크업/스타일 패턴과 공통 레이아웃 요구사항(헤더, 콘텐츠, 사이드 패널 등)을 문서화하여 반응형 재구성의 기반을 마련.
- 추가 지시: **모바일 전용 CSS/DOM 분기를 모두 제거하고 데스크톱 기준 단일 마크업을 반응형으로 축소되도록 재작성**해야 한다. 즉, 윈도우 폭이 줄어들면 동일한 요소가 뷰포트 크기에 맞춰 유연하게 변형되어야 하며, 모바일 전용 컴포넌트는 유지하지 않는다.

## 주요 모바일 전용 패턴
### 1. 테이블형 데이터 페이지
- `NewDbListPage`, `MyDbListPage`, `MyAssignmentHistoryPage`, `StarredDbListPage` 등은 데스크톱용 `<table>` 과 별도의 `.mobile-card-view` 블록을 동시에 유지한다.【F:src/NexaCRM.UI/Pages/NewDbListPage.razor†L24-L71】
- 동일 패턴이 `ContactsPage`에서도 나타나며, 모바일 카드가 데스크톱 테이블과 동일 데이터를 중복 렌더링한다.【F:src/NexaCRM.UI/Pages/ContactsPage.razor†L45-L147】
- CSS에서는 `.desktop-table-view` 와 `.mobile-card-view` 토글을 위해 `@media (max-width: 767.98px)` 또는 유사한 한계값을 사용한다. 다수의 리스트 페이지가 동일한 임계값을 공유하므로 공통 반응형 테이블/카드 컴포넌트화 필요성이 높다.
- **필수 조치:** 모바일 카드 DOM을 삭제하고, 동일 `<table>` 또는 `<div>` 그리드가 뷰포트에 따라 열 수·간격만 조정하도록 `display: grid/flex` 변환과 열 숨김 유틸리티를 정의한다.

### 2. 대시보드 및 헤더 상호작용
- `TestDashboard`는 데이터 중복 없이 모바일 헤더(`data-mobile-header`), 퀵 액션 바, 알림 패널을 별도 DOM 섹션으로 정의한다.【F:src/NexaCRM.UI/Pages/TestDashboard.razor†L12-L136】
- `ManagerDashboard`, `ProfileSettingsPage` 등도 모바일 메뉴 버튼, 오버레이, 사이드 내비게이션을 개별적으로 구현하고 있으며, 유사한 구조를 반복한다.【F:src/NexaCRM.UI/Pages/ProfileSettingsPage.razor†L9-L144】
- 이러한 헤더/내비게이션 패턴은 공통 브레이크포인트(대개 768px)에서 토글되며, 재사용 가능한 모바일 헤더 컴포넌트 정의가 필요하다.
- **필수 조치:** 별도 모바일 헤더/내비게이션 DOM 조각을 제거하고, 하나의 헤더 컴포넌트가 CSS 유틸리티(예: `.is-collapsed`, `.is-expanded`)를 통해 폭에 따라 전환되도록 설계한다.

### 3. 설정 및 폼 기반 페이지
- `ProfileSettingsPage`, `NotificationSettingsPage`, `SecuritySettingsPage` 등은 데스크톱 기준의 2열/고정폭 폼을 구성한 뒤 모바일에서는 패딩 축소와 스택 정렬을 위해 별도 `@media` 규칙과 모바일 메뉴를 사용한다.【F:src/NexaCRM.UI/Pages/ProfileSettingsPage.razor†L67-L144】
- 레이아웃 컨테이너(`.layout-container`, `.layout-content-container`)와 사이드 내비게이션/오버레이 패턴이 반복되므로, 공통 폼 레이아웃 유틸리티와 모달/오버레이 토글이 요구된다.
- **필수 조치:** 중복 모바일 전용 `@media` 규칙을 제거하고, `grid-template-columns`, `minmax()` 등을 활용한 반응형 폼 레이아웃 토큰을 도입한다.

### 4. 컨테이너 쿼리 기반 테이블 축소
- 일부 페이지(`CustomerSupportDashboard`, `SettingsPage`, `SalesPipelinePage` 등)는 `@container(max-width: Xpx)` 규칙으로 컬럼 표시/숨김을 제어한다.【F:src/NexaCRM.UI/Pages/CustomerSupportDashboard.razor.css†L1-L5】
- 컨테이너 기반 접근과 뷰포트 기반 `@media` 규칙이 혼재하므로, 공통 브레이크포인트 정의와 우선순위 조정이 필요하다.
- **필수 조치:** 컨테이너/뷰포트 혼합 전략을 통합하여 단일 레이아웃이 폭 축소 시 자연스럽게 열을 숨기거나 래핑하도록 공통 믹스인을 정의한다.

## 브레이크포인트 및 반응형 토큰 현황
| 구분 | 사용 예시 | 비고 |
| --- | --- | --- |
| ≤ 480px, 479.98px | `TestDashboard.razor.css`, `DemoContactsPage.razor.css` | 모바일 최적화(폰). |
| ≤ 575/576px | `SmsSchedulePage.razor.css`, `BizManagementPage.razor.css` | Bootstrap `sm` 경계 참조. |
| ≤ 640px | `EmailTemplateBuilder.razor.css`, `SettingsPage.razor.css` | Tailwind `md` 유사. |
| ≤ 768px, 767.98px | 대다수 테이블/헤더 전환 | 태블릿 전환의 사실상 표준. |
| ≤ 900/991/1024px | `ReportsPage.razor.css`, `DbAdvancedManagementPage.razor.css` | 중형 화면 대응. |
| ≥ 769px & ≤1024px 등 | `SalesCalendar.razor.css` | 특정 뷰 전환용 범위 미디어 쿼리. |
| 컨테이너 쿼리 120~840px | 여러 테이블 페이지 | 열 숨김/카드 전환에 사용. |

> 동일 기능에 다양한 뷰포트 한계가 혼재하여 유지보수 비용이 높음. 통일된 디자인 토큰(예: `--breakpoint-sm`, `--breakpoint-md`) 정의 후 공통 믹스인을 제공해야 함.

## 공통 레이아웃 요구사항 요약
1. **헤더 및 내비게이션**
   - 데스크톱: 고정 헤더 + 수평 메뉴.
   - 모바일: 햄버거 버튼, 오버레이, 토글 가능한 사이드 메뉴 필요.【F:src/NexaCRM.UI/Pages/ProfileSettingsPage.razor†L39-L66】【F:src/NexaCRM.UI/Pages/TestDashboard.razor†L14-L99】
   - 요구사항: 동일한 컴포넌트에서 메뉴 토글, 오버레이, 포커스 트랩 등을 지원하는 반응형 헤더 모듈.

2. **콘텐츠 레이아웃**
   - 리스트/테이블: 행 기반 목록을 카드 레이아웃으로 전환하는 재사용 가능한 패턴이 필요.【F:src/NexaCRM.UI/Pages/NewDbListPage.razor†L24-L70】【F:src/NexaCRM.UI/Pages/ContactsPage.razor†L45-L147】
   - 대시보드: 위젯 그리드와 퀵 액션 바가 뷰포트에 따라 열 수 및 고정 패널을 조정해야 함.【F:src/NexaCRM.UI/Pages/TestDashboard.razor†L60-L180】
   - 폼: 입력 폭 제한(`ui-max-w-480`, `max-width` 인라인 스타일 등)과 스택 정렬 필요.【F:src/NexaCRM.UI/Pages/ProfileSettingsPage.razor†L75-L144】

3. **사이드 패널 및 오버레이**
   - 모바일 메뉴(`profile-mobile-nav`, `contacts-mobile-nav`), 알림 패널(`mobile-notifications-panel`) 등이 각 페이지별로 별도 구현됨.【F:src/NexaCRM.UI/Pages/ProfileSettingsPage.razor†L50-L66】【F:src/NexaCRM.UI/Pages/TestDashboard.razor†L91-L135】
   - 요구사항: 토글 가능한 사이드 패널/오버레이를 위한 통합 유틸리티(ARIA 속성, 스크롤 잠금) 구축.

4. **상태 동기화**
   - 동일 데이터셋을 데스크톱/모바일 두 레이아웃에 중복 렌더링할 경우 이벤트/상태 유지 부담이 증가함. 반응형으로 통합할 경우 상태 관리 단일화 필요.【F:src/NexaCRM.UI/Pages/ContactsPage.razor†L45-L147】

## 후속 단계 제안
- 디자인 토큰 및 반응형 유틸리티를 `src/NexaCRM.UI/wwwroot/css` 내 공용 스타일로 정의하여 브레이크포인트·spacing을 일원화.
- 테이블 ↔ 카드 전환, 모바일 헤더, 오버레이 모듈 등 재사용 가능한 Blazor 컴포넌트/Partial Layout을 식별하고 정의 계획 수립.
- 모바일 전용 CSS/마크업 제거 작업 목록을 작성하고, 각 페이지가 단일 데스크톱 마크업을 기반으로 `@media (max-width: var(--breakpoint-md))` 등 공통 토큰만 사용하도록 린트 규칙/코드리뷰 체크리스트를 마련.
- QA 시나리오: 375px/768px/1024px/1280px 뷰포트에서 헤더, 테이블, 폼, 대시보드 동작 확인.


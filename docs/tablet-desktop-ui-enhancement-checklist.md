# 태블릿·데스크탑 UX/UI 고도화 진단 리포트

## 0. 맥락 파악 (사전 진단)
### 진단
* `MainDashboard`는 모바일 헤더·퀵 액션·알림 패널을 항상 렌더링하고 그 아래에 데스크탑 사이드바와 콘텐츠 래퍼를 두어 뷰포트에 따라 서로 겹치는 구조입니다. 태블릿·데스크탑 전환 시 어떤 계층을 숨기거나 재배치할지 명확한 규칙이 없어 시각적 충돌과 중복 내비게이션이 발생할 수 있습니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor†L12-L195】
* 전역 `ResponsivePage` 래퍼는 부트스트랩 그리드 한 컬럼만 감싸고, `mobile.css`는 768px 이상에서 최대 폭 800px만 지정해 태블릿 폭 확장이나 멀티컬럼 구성이 제한됩니다.【F:src/Web/NexaCRM.WebClient/Shared/ResponsivePage.razor†L1-L8】【F:src/Web/NexaCRM.WebClient/wwwroot/css/mobile.css†L339-L349】
* `StatisticsDashboardPage`는 접근성 있는 퀵 범위 버튼, 요약 타일, SVG 차트 뼈대를 제공하지만 데스크탑 환경에서의 데이터 밀도·상호작용·애니메이션 정의는 비어 있습니다.【F:src/Web/NexaCRM.WebClient/Pages/StatisticsDashboardPage.razor†L10-L220】
### 제안
* 화면 크기별로 모바일 헤더/패널과 데스크탑 사이드바의 우선순위를 문서화하고, 공통 툴바 또는 스위처 패턴을 지정합니다.
* 태블릿(≥768px)과 데스크탑(≥1200px)용 최대 폭, 패딩, 그리드 토큰을 재설계하여 `ResponsivePage`와 전역 CSS에 반영합니다.
* 통계 대시보드의 주요 지표·차트 유형·상호작용 수준을 정량화해 추후 컴포넌트 고도화의 기준선으로 삼습니다.

## 1. 레이아웃 & 반응형 시스템
#### 1.1 공통 체크
* 진단: 대시보드 영역은 Tailwind 유틸(`w-80`, `flex`, `gap`) 중심으로 구성되어 있고, 기기별 컨테이너 폭이나 컬럼 수를 제어하는 디자인 토큰이 없습니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor†L156-L195】
* 제안: CSS 커스텀 프로퍼티/유틸을 정의해 태블릿·데스크탑에서 12컬럼 기반 레이아웃과 섹션 간 공통 스페이싱을 표준화합니다.
#### 1.2 태블릿(768–1199px)
* 진단: 모바일 퀵 액션 바가 고정 렌더링되고, 사이드바 폭 `w-80`(≈320px)과 본문 영역 구분이 부족해 태블릿에서 비좁은 레이아웃이 예상됩니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor†L62-L102】【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor†L156-L195】
* 제안: 태블릿에서는 퀵 액션을 콘텐츠 상단 카드로 이동하고, 접힘 가능한 네비게이션 레일과 1~2열 카드 배치를 재구성합니다.
#### 1.3 데스크탑(≥1200px)
* 진단: 데스크탑 특정 `max-width` 제어가 없어 초광폭 모니터에서 콘텐츠가 과도하게 넓어질 수 있고, 멀티패널 구성이나 3열 그리드 정의가 없습니다.【F:src/Web/NexaCRM.WebClient/Shared/ResponsivePage.razor†L1-L8】
* 제안: 1200~1440px 기본 최대 폭과 1600px 이상 확장 전략을 문서화하고, 사이드바·필터·알림 패널 동시 노출이 가능한 멀티컬럼 레이아웃을 설계합니다.

## 2. 내비게이션 & 정보 구조
### 진단
* 사이드 네비게이션은 `AuthorizeView` 기반으로 역할별 링크를 노출하지만, 활성 상태·뱃지·접힘 인터랙션이 정의되지 않았습니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor†L160-L195】
* 모바일 퀵 액션 버튼과 데스크탑 사이드바가 동일 목적의 링크를 중복 제공하며, 전역 검색·알림·설정 흐름이 기기별로 분리되어 있습니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor†L26-L118】
### 제안
* 태블릿 이상에서 네비게이션 레일+헤더 단축 버튼 체계를 정의하고 권한·상태 뱃지, 현재 위치 표시를 명확히 합니다.
* 전역 검색/알림/설정 우선순위, 포커스 순서, 스킵 링크 등 내비게이션 흐름을 키보드 기준으로 문서화합니다.

## 3. 대시보드 & 차트 고도화
### 진단
* 요약 타일과 추세 차트는 SVG 기반 시각화를 제공하지만, 실시간 업데이트 피드백·툴팁·범례 토글 등 상호작용 요소가 없습니다.【F:src/Web/NexaCRM.WebClient/Pages/StatisticsDashboardPage.razor†L74-L220】
* 로딩 단계는 `LoadingSpinner`만 호출하고 차트 스켈레톤이나 애니메이션 규칙은 정의되어 있지 않습니다.【F:src/Web/NexaCRM.WebClient/Pages/StatisticsDashboardPage.razor†L63-L71】【F:src/Web/NexaCRM.WebClient/Components/UI/LoadingSpinner.razor†L3-L170】
### 제안
* KPI 타일에 상승/하락 애니메이션, 목표 대비 지표, 최신 업데이트 타임스탬프를 추가하고, 차트에 호버·포커스 툴팁·평균선·범례 토글을 설계합니다.
* 로딩과 실시간 갱신 상태에 스켈레톤/펄스/프로그레시브 라인 드로잉 애니메이션을 도입합니다.

## 4. 데이터 테이블·폼 경험
### 진단
* `ReportsPage`는 필드 추가/필터/미리보기 테이블을 단일 컬럼으로 배치해, 태블릿 이상에서 폼과 목록을 동시에 다루기 어렵습니다.【F:src/Web/NexaCRM.WebClient/Pages/ReportsPage.razor†L10-L65】
* 인라인 검증·저장/취소 영역 고정·스텝바이저드 흐름 등 편집 UX 요소가 부재합니다.【F:src/Web/NexaCRM.WebClient/Pages/ReportsPage.razor†L10-L115】
### 제안
* 태블릿에서는 폼과 저장 목록을 2열로 배치하고, 데스크탑에서는 필드 그룹/필터 패널을 구분해 인라인 검증, 고정 액션 영역, 드릴다운 가능한 테이블을 설계합니다.

## 5. 컴포넌트 & 상호작용 모듈
### 진단
* `QuickActionsComponent`는 기기 감지 후 `tel:`/`mailto:`/알림 창 호출만 제공해 다단계 확장이나 통합 피드백이 부족합니다.【F:src/Web/NexaCRM.WebClient/Components/UI/QuickActionsComponent.razor†L6-L134】
* 로딩 스피너는 스켈레톤/프로그레스 옵션을 지원하지만 레이아웃·색상 토큰 연동이 미흡합니다.【F:src/Web/NexaCRM.WebClient/Components/UI/LoadingSpinner.razor†L3-L200】
### 제안
* 데스크탑 회의 예약·알림 전송은 모달/슬라이드오버·툴팁·성공/실패 토스트와 연계하고, 액션 완료 이벤트를 대시보드 카드 상태와 연결합니다.
* 필터 패널·태그 바·세그먼트 컨트롤 등 반복 UI를 컴포넌트화하고 상태별 스타일 가이드를 문서화합니다.

## 6. 애니메이션 & 마이크로 인터랙션
### 진단
* 네비게이션 링크와 버튼은 0.2초 컬러 전환만 정의되어 등장/퇴장, 상태 변화에 대한 모션 스케일이 없습니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor.css†L1-L37】
### 제안
* 등장/퇴장 0.3~0.4초 이징 커브, 스크롤 연동 페이드/슬라이드, 탭 전환 모션, 토스트 진입/퇴장 애니메이션을 기기별로 세분화하고 `prefers-reduced-motion` 대응을 포함합니다.

## 7. 시각 디자인 & 테마
### 진단
* 다크 모드용 색상 재정의는 존재하지만, 전역 토큰과 차트/버튼/태그 간 일관성·명암비 기준이 정리되어 있지 않습니다.【F:src/Web/NexaCRM.WebClient/Pages/MainDashboard.razor.css†L81-L199】
### 제안
* 브랜드/상태 색상, 그림자, 타이포그래피 스케일을 기기별로 조정하고 다크 모드 대비 4.5:1 이상을 보장하는 팔레트, KPI용 대체 폰트 적용 지침을 마련합니다.

## 8. 접근성 & 국제화
### 진단
* 통계 대시보드 툴바는 ARIA 역할·라이브 영역을 일부 정의했으나 키보드 포커스 스타일, 스킵 링크, 언어 길이 변동 대응은 부족합니다.【F:src/Web/NexaCRM.WebClient/Pages/StatisticsDashboardPage.razor†L19-L59】
### 제안
* 포커스 상태, 스킵 링크, ARIA 속성(`aria-live`, `aria-pressed`, `aria-expanded`)을 전역 컴포넌트에 확장하고 다국어 문자열 길이에 맞춘 버튼 최소 폭·차트 축 레이아웃을 설계합니다.

## 9. 성능 & 기술 고려 사항
### 진단
* 통계 페이지는 서버 호출 후 전역 로딩 스피너만 표시하며, 데이터 스트리밍·프로그레시브 하이드레이션·가상 스크롤 등 고해상도 기기 성능 최적화 전략은 마련되지 않았습니다.【F:src/Web/NexaCRM.WebClient/Pages/StatisticsDashboardPage.razor†L63-L205】
* 퀵 액션은 브라우저 `alert` 호출에 의존해 비동기 상태 관리나 사용자 선호 저장 전략이 없습니다.【F:src/Web/NexaCRM.WebClient/Components/UI/QuickActionsComponent.razor†L74-L134】
### 제안
* 대시보드 데이터는 스트리밍/청크 로딩, 리소스 분할, LCP ≤2.5초 목표를 위한 CSS/JS 최적화를 문서화하고, 차트·테이블에는 가상 스크롤·부분 업데이트를 도입합니다.
* 사용자 맞춤 위젯 순서/보기 설정 저장·복원을 위한 상태 관리 및 스토리지 전략을 정의합니다.

> 이 문서는 태블릿·데스크탑 UI 고도화 시 발견된 현황과 개선 포인트를 번호 순서(0~9)로 정리한 것으로, 디자인 시스템·개발 태스크 우선순위 산정에 직접 활용할 수 있도록 구성했습니다.

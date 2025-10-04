# 공지사항 서비스 & UI 설계 개요

## 1. 개요
- 고객센터 공지사항은 Supabase `customer_notices` 테이블을 단일 진실 소스로 사용합니다.
- Blazor WebAssembly 및 Blazor Server(WebServer) 호스트 모두에서 `INoticeService` 구현체로 `SupabaseNoticeService`를 주입해 CRUD를 수행합니다.
- UI는 데스크톱/모바일 반응형 카드 리스트와 상세 패널의 투 페인(two-pane) 구성으로 설계했습니다.

## 2. 데이터 계층
### 2.1 테이블 스키마 확장
- 컬럼: `title`, `summary`, `content`, `category`, `importance`, `is_pinned`, `published_at`, `tenant_id`, `reference_url`.
- `category`는 `NoticeCategory` 열거형(`General`, `Update`, `Maintenance`, `Security`, `Policy`, `Promotion`)과 매핑됩니다.
- `importance`는 `NoticeImportance` 열거형(`Normal`, `Highlight`, `Critical`)과 매핑되어 배지 스타일을 제어합니다.
- `is_pinned`는 상단 고정 여부를 컨트롤하며 리스트 정렬에 반영됩니다.

### 2.2 PostgREST 모델
- `CustomerNoticeRecord`는 Supabase.Postgrest `BaseModel`을 상속하며 컬럼을 속성으로 매핑합니다.
- 날짜는 `DateTime`으로 역직렬화된 뒤 UTC로 강제 지정 후 `DateTimeOffset`으로 변환합니다.
- `ReferenceUrl`은 선택적 외부 링크를 제공하며 상세 패널에서 새 탭으로 연결합니다.

## 3. 서비스 계층
### 3.1 SupabaseNoticeService
- `SupabaseClientProvider`를 통해 구성/세션을 공유하며, DI 주입 시 null 보호를 수행합니다.
- `GetNoticesAsync`는 `is_pinned` → `published_at` 순으로 정렬하여 리스트를 반환합니다.
- `Create`, `Update`, `Delete`는 PostgREST 연산(`Insert`, `Update`, `Delete`)을 직접 호출하며 예외 발생 시 `ILogger`로 에러를 로깅합니다.
- Enum 파싱은 대소문자 무시(`Enum.TryParse(..., true, out ...)`)를 사용해 문자열과 안전하게 상호 변환합니다.

### 3.2 Fallback 시나리오
- Supabase 연결이 불가능한 통합 테스트나 독립 실행 환경에서는 기존 `NoticeService`를 통해 인메모리 데이터를 제공합니다.
- `CustomerCenterService` 역시 새 모델 구조를 반영하여 데모 데이터를 유지합니다.

## 4. 프레젠테이션 계층
### 4.1 NoticeManagementPage
- 검색 입력은 `@bind:event="oninput"`으로 실시간 필터링하며, 필터/검색 변경 시 내부 캐시를 무효화합니다.
- 카테고리 칩은 `FilterOptions` 튜플 컬렉션으로 정의되어 유지보수가 용이합니다.
- 리스트/상세 2열 레이아웃은 CSS Grid로 구성, 모바일 구간에서는 단일 컬럼으로 전환됩니다.
- 선택된 공지사항은 상세 패널에 제목/배지/게시일/본문 단락을 표시하며, 외부 링크가 있을 경우 버튼을 노출합니다.

### 4.2 스타일 가이드
- `NoticeManagementPage.razor.css`는 프로젝트의 모던 팔레트(`--primary-color`, `--surface-muted`, `--text-secondary`)를 활용합니다.
- 칩 배지 색상은 카테고리/중요도별로 지정되어 시각적 위계를 제공합니다.
- 로딩/오류/빈 상태 UI를 제공해 사용자 경험을 보완합니다.

## 5. 향후 확장 포인트
- Supabase Realtime 구독을 추가해 신규 공지 등록 시 리스트를 자동 갱신할 수 있습니다.
- `status` 컬럼을 도입하면 예약 게시/비공개 상태를 제어할 수 있으며, UI에 필터를 추가할 수 있습니다.
- Markdown 렌더러를 연결해 풍부한 텍스트 표현을 지원할 수 있습니다.

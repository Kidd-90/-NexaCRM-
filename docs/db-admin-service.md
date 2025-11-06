# DB Admin Service Overview

`DbAdminService`는 관리자용 DB 고객 관리 화면에서 필요한 핵심 백엔드 유틸리티를 제공합니다. 본 문서는 구현된 기능과 활용 포인트를 간략히 정리합니다.

## 제공 기능
- **검색(SearchAsync)**
  - 배정일 기간(`From`, `To`), 상태(`Status`), 텍스트 검색(`SearchTerm`)을 조합해 고객 목록을 조회합니다.
  - 연락처 번호를 숫자만 비교하여 중복 레코드를 감지하고, 필요 시 중복 항목만 반환합니다.
  - `IncludeArchived`를 `true`로 지정하면 보관된(Archived) 항목까지 포함할 수 있습니다.
- **내보내기(ExportToExcelAsync)**
  - 선택한 필드 집합을 기반으로 CSV 포맷(`UTF-8`) 데이터를 생성합니다.
  - 필드 지정이 없을 경우 `CustomerName`, `ContactNumber`, `Group`, `AssignedTo`, `AssignedDate`, `Status`, `LastContactDate` 순으로 기본 열을 출력합니다.
  - 동일한 검색 조건(`DbSearchCriteria`)을 함께 전달하면 UI에 표시된 필터와 동일한 결과만 내보낼 수 있습니다.
- **삭제(DeleteEntryAsync)**
  - `IDbDataService.DeleteCustomersAsync`를 호출해 지정된 고객(ContactId) 레코드를 제거합니다.

## 연계 서비스
- `IDbDataService`
  - 모든 동작이 Supabase 기반 `SupabaseDbDataService`와 연계되며, 오프라인/테스트 용도의 인메모리 구현도 동일 계약을 따릅니다.
  - 서비스 DI 구성에서 `SupabaseClientProvider`가 초기화되고 올바른 Supabase URL/Anon Key가 주입되어야 합니다.
- `DuplicateService`
  - `SearchAsync`의 중복 탐지 로직은 `DuplicateService`와 동일하게 연락처 번호를 기준으로 하며, 향후 필요 시 고급 스코어링 로직으로 확장할 수 있습니다.

## 테스트
- `tests/NexaCRM.Service.Tests/DbAdminServiceTests.cs`
  - 날짜/중복/상태/검색어 필터링, CSV 내보내기, 삭제 위임이 올바르게 동작하는지 단위 테스트로 검증합니다.

## 전화번호 정규화 유틸리티
- `PhoneNumberNormalizer`
  - 관리자 서비스 전반에서 연락처 문자열에서 숫자만 추출하는 공용 도구입니다.
  - `DbAdminService` 검색/중복 필터, `DuplicateService` 중복 그룹핑이 모두 동일한 정규화 규칙을 따릅니다.
  - 내부적으로 `Span<char>`와 `stackalloc`을 활용해 짧은 문자열 처리 시 힙 할당을 줄여 성능 회귀를 방지합니다.
- 추가 검증
  - `tests/NexaCRM.Service.Tests/DuplicateServiceTests.FindDuplicatesAsync_NormalizesContactNumbers`가 포맷이 다른 연락처도 동일하게 묶이는지 보증합니다.

## UI 연동 현황
- `src/NexaCRM.UI/Pages/DbAdvancedManagementPage.razor`
  - 초기 로딩 및 필터링, 삭제, CSV 내보내기를 `IDbAdminService`를 통해 수행합니다.
  - 검색어/상태/기간 필터는 UI에서 즉시 적용되며, 동일 조건으로 내보내기 기능을 이용할 수 있습니다.

## 호스트 DI 등록 위치
- **Blazor Server**: `src/NexaCRM.WebServer/Startup.cs`의 `ConfigureServices`에서 `services.AddNexaCrmAdminServices()`를 호출하면 서버 호스트가 `SupabaseDbDataService`와 `DbAdminService`를 포함한 관리자 서비스를 공용 DI 컨테이너에 등록합니다. `Configure` 단계에서는 `ValidateAdminServices`를 실행해 필수 의존성이 빠져 있을 경우 즉시 예외를 발생시키므로, 잘못된 DI 구성을 조기에 파악할 수 있습니다.
- **Blazor WebAssembly**: `src/NexaCRM.WebClient/Program.cs`에서 `builder.Services.AddNexaCrmAdminServices()`를 호출하면 WebAssembly 호스트에서도 동일한 Supabase 데이터 소스가 주입됩니다. 추가 Mock 재등록 없이도 클라이언트와 서버가 동일한 DB 데이터를 참조합니다.

## Supabase 연동 참고
- 실제 테이블은 `supabase/migrations/schema.sql`에 정의된 `db_customers` 스키마와 매핑되며, `SupabaseDbDataService`는 PostgREST API를 통해 CRUD 및 병합 작업을 수행합니다.
- Supabase 환경값이 비어 있는 경우 `AddSupabaseClientOptions`가 오프라인 URL/Anon Key로 폴백하지만, 실서비스 배포 시 반드시 유효한 키를 주입해야 합니다.

## 다음 작업 백로그

| 우선순위 | 영역 | 작업 내용 | 완료 조건 |
|----------|------|-----------|------------|
| 🔴 높음 | 백엔드 | `SupabaseDbDataService` 파이프라인에서 신규/갱신 고객에 `PhoneNumberNormalizer`를 강제 적용해 저장 전 연락처 포맷을 일관화합니다. | 통합 테스트에서 저장된 연락처가 모두 숫자 문자열로 정규화됨 |
| 🟠 중간 | 프런트엔드 | Blazor UI 검색 입력 전처리를 백엔드 규칙과 맞추는 가이드 문서를 작성하고 `Shared` 컴포넌트에 실제 반영 여부를 점검합니다. | 가이드 문서 초안 + 관련 컴포넌트 PR 링크 |
| 🟡 보통 | 성능 | 10만 건 이상의 더미 데이터로 `PhoneNumberNormalizer` 호출 빈도와 처리 시간을 계측하고 캐싱/사전 정규화 전략의 필요성을 평가합니다. | 측정 리포트 및 권고안 작성 |
| 🟢 낮음 | QA 자동화 | `DuplicateService` 병합 결과를 Playwright 시나리오 또는 통합 테스트로 자동 검증하여 회귀를 방지합니다. | CI에서 자동화 시나리오가 실행되고 성공 |

### 진행 방식 제안
- **백엔드/프런트엔드** 항목은 각각 담당 팀원이 소유하며, 완료 조건 충족 시 체크리스트 업데이트를 통해 공유합니다.
- **성능/QA** 항목은 주간 회의에서 진행 현황을 공유하고, 필요 시 추가 리소스(프로파일링 도구, 테스트 계정)를 확보합니다.

> 이 문서는 DbAdminService 기능 확장 시 최신 상태를 유지하기 위해 관리합니다.

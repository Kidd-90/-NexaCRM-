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

## 다음 작업 리스트
1. **Supabase 동기화 단계 점검**
   - 신규 고객 삽입/갱신 시 백엔드 레이어에서도 `PhoneNumberNormalizer`를 적용하도록 `SupabaseDbDataService` 파이프라인을 검토합니다.
2. **프런트엔드 검색 정렬**
   - Blazor UI의 검색어 전처리 로직을 백엔드 정규화 규칙과 맞추는 가이드 문서를 추가하고, Shared 컴포넌트에 적용 여부를 확인합니다.
3. **성능 모니터링**
   - 대량(>10만 건) 데이터셋에서 `PhoneNumberNormalizer` 호출 빈도를 샘플링하고, 필요 시 캐싱 또는 사전 정규화 전략을 실험합니다.
4. **중복 병합 QA 자동화**
   - `DuplicateService`의 병합 결과가 예상대로 동작하는지 통합 테스트 또는 Playwright 시나리오를 추가해 회귀를 예방합니다.

> 이 문서는 DbAdminService 기능 확장 시 최신 상태를 유지하기 위해 관리합니다.

# NexaCRM 디자인·서비스·아키텍처 고도화 점검 리포트

## 1. 개요
- **목적**: Blazor 기반 NexaCRM 솔루션의 UI/UX 디자인, 서비스 계층, 전체 아키텍처를 세 축으로 나누어 고도화 수준을 진단하고 후속 우선순위를 제시합니다.
- **평가 범위**: `NexaCRM.UI` 컴포넌트와 스타일, WebAssembly 호스트의 Supabase 연동 서비스, 솔루션 전체 프로젝트 구조 및 문서화 상태를 검토했습니다.

## 2. 디자인 고도화 진단
| 항목 | 현재 성숙도 | 근거 | 우선 과제 |
| --- | --- | --- | --- |
| 반응형 레이아웃 | ⚠️ 중간 | 모바일 헤더·퀵액션·알림 패널과 데스크탑 사이드바가 동시에 렌더링되어 중복 내비게이션이 발생하고, 컨테이너 폭·그리드 토큰 정의가 미흡합니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L3-L27】 | 뷰포트 구간별 레이아웃 규칙, 공통 디자인 토큰(폭, 패딩, 컬럼 수) 정의 |
| 내비게이션 & 정보 구조 | ⚠️ 중간 | 역할 기반 링크는 있으나 상태 뱃지·접힘 패턴이 없고, 모바일/데스크탑 전환 시 경로가 중복됩니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L24-L31】 | 사이드바·헤더 통합 전략, 권한 뱃지·포커스 흐름 명시 |
| 대시보드 시각화 | ⚠️ 초기 | SVG 기반 지표/차트 골격은 있으나 실시간 상호작용·애니메이션·로딩 스켈레톤이 정의되지 않았습니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L32-L39】 | KPI 타일 상호작용, 차트 툴팁/범례, 로딩/갱신 모션 추가 |
| 데이터 입력/테이블 UX | ⚠️ 초기 | Reports 페이지가 단일 컬럼·기본 검증만 제공해 태블릿 이상의 작업 흐름이 비효율적입니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L40-L46】 | 2열 폼+리스트 배치, 고정 액션 영역, 단계별 검증 설계 |
| 공통 컴포넌트 & 모션 | ⚠️ 초기 | 퀵액션은 단순 링크 호출, 네비게이션 모션·상태 토큰이 제한적입니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L47-L59】 | 액션 완료 피드백, 토스트/모달 패턴, 모션 스케일 가이드 확립 |

### 추가 인사이트
- `MainDashboard` 페이지는 모바일·데스크탑 요소를 모두 포함하는 단일 뷰라서 기기별 레이아웃 분리를 위한 컴포지션 리팩터링이 필요합니다.【F:src/NexaCRM.UI/Pages/MainDashboard.razor†L18-L199】
- Tailwind 유틸 기반 스타일이 많아 토큰화·테마화가 늦춰지고 있으므로 UI 전역에서 재사용 가능한 디자인 시스템 계층 도입이 권장됩니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L13-L23】

## 3. 서비스(애플리케이션 계층) 고도화 진단
| 항목 | 현재 성숙도 | 근거 | 우선 과제 |
| --- | --- | --- | --- |
| Supabase 클라이언트 구성 | ✅ 기반 확보 | Program에서 Supabase 옵션 검증, 세션 지속성, AutoRefreshToken 설정을 마친 상태입니다.【F:src/NexaCRM.WebClient/Program.cs†L34-L103】 | 실서비스 환경 변수 주입, Realtime 채널 구성 재검토 |
| 핵심 도메인 CRUD | ⚠️ 진행 중 | 연락처 서비스는 Supabase SDK로 CRUD를 구현했지만 예외 전파 이후 사용자 피드백 계층이 미정입니다.【F:src/NexaCRM.WebClient/Services/SupabaseContactService.cs†L25-L80】 | UI 예외 처리, 캐싱/페이지네이션, 테스트 커버리지 확대 |
| 인증·세션 | ⚠️ 착수 전 | `CustomAuthStateProvider`는 등록돼 있으나 Supabase Auth 기반으로 교체하는 작업은 체크리스트에서 미완료로 남아 있습니다.【F:docs/supabase-service-web-task-plan.md†L15-L22】 | Supabase Auth API 연동, 토큰 자동 갱신·로그아웃 플로우 구현 |
| 조직/역할 관리 | ❌ 미착수 | 조직·역할 동기화, 권한 기반 UI 노출 개선이 체크리스트에서 모두 미완료입니다.【F:docs/supabase-service-web-task-plan.md†L31-L38】 | Supabase `organization_users` 연동, 역할 기반 표시·필터링 |
| 실시간/알림 연동 | ❌ 미착수 | Realtime 채널 연결과 장애 대응 로직이 문서에서 미완료로 남아 있습니다.【F:docs/supabase-service-web-task-plan.md†L39-L46】 | Realtime 구독, 재연결 전략, UI 실시간 알림 반영 |
| 스토리지·엣지 기능 | ❌ 미착수 | 스토리지 업로드/다운로드, Edge Function 피드백이 모두 계획 단계입니다.【F:docs/supabase-service-web-task-plan.md†L47-L61】 | 버킷 설계, 업로드 UX, Edge Function 상태 표시 |

### 추가 인사이트
- 현재 Supabase 서비스 구현은 브라우저에서 직접 호출하는 패턴으로 설계되어 있어, 보안 강화를 위해 서버 프록시(API Gateway) 또는 Edge Function 기반 데이터 접근 전략을 병행 검토해야 합니다.
- 예외 처리 시 `throw;`로 재전파 후 UI 계층에서 사용자 피드백이 제공되지 않아, 표준화된 오류 메시지·재시도 전략을 컴포넌트 레벨에서 정의해야 합니다.【F:src/NexaCRM.WebClient/Services/SupabaseContactService.cs†L42-L78】

## 4. 아키텍처 고도화 진단
| 항목 | 현재 성숙도 | 근거 | 우선 과제 |
| --- | --- | --- | --- |
| 멀티 호스트 구조 | ✅ 안정 | Blazor Server·WebAssembly가 `NexaCRM.UI`와 `NexaCRM.Service`를 공유하는 구조가 명확히 문서화돼 있습니다.【F:docs/architecture/blazor-hosting-alignment.md†L8-L37】 | 호스트별 특화 기능은 DI 추상화로 분리 유지 |
| 도메인 계층화 | ⚠️ 중간 | `NexaCRM.Service`에 Mock/Core 구현이 존재하나, 실서비스 연동을 위한 API Gateway·백엔드 계약은 아직 비어 있습니다.【F:docs/architecture/blazor-hosting-alignment.md†L13-L20】【F:src/NexaCRM.ApiGateway/ApiGateway.cs†L1-L6】 | API Gateway 구현, 서버 측 Supabase 래핑 서비스 도입 |
| 운영/테스트 전략 | ⚠️ 중간 | 모니터링·백업 가이드는 마련됐지만 CI/CD에서 Supabase 스키마 배포·통합 테스트 자동화는 미완료입니다.【F:docs/supabase-service-web-task-plan.md†L63-L88】 | Supabase CLI 기반 배포 파이프라인, 통합 테스트 작성 |
| 디자인 문서화 | ✅ 기반 확보 | 태블릿·데스크탑 UI 고도화 체크리스트 등 상세 문서가 존재해 협업 기반은 마련됐습니다.【F:docs/tablet-desktop-ui-enhancement-checklist.md†L1-L81】 | 문서와 실제 구현 동기화 주기 관리 |

### 아키텍처 고도화 제언
1. **API Gateway 실장**: 현재 빈 껍데기인 `NexaCRM.ApiGateway` 프로젝트에 Supabase 서비스 역할 키를 사용하는 BFF 패턴을 도입해 브라우저로부터 민감한 키를 격리합니다.【F:src/NexaCRM.ApiGateway/ApiGateway.cs†L1-L6】
2. **계층 테스트 전략**: `NexaCRM.Service`에서 Supabase 연동 인터페이스를 정의하고, 통합 테스트 프로젝트를 추가해 RLS 및 데이터 일관성을 검증합니다.【F:docs/supabase-service-web-task-plan.md†L71-L78】
3. **공유 UI 자산 강화**: `NexaCRM.UI`에서 디자인 토큰·모션 시스템을 모듈화해 WebClient/WebServer 간 일관성을 높이고, 다국어 리소스/테마 동기화를 자동화합니다.【F:docs/architecture/blazor-hosting-alignment.md†L8-L25】

## 5. 단기 우선순위 로드맵
1. **디자인**: 레이아웃 토큰·네비게이션 가이드 정의 → 대시보드 상호작용 강화 → 데이터 입력 UX 재구성.
2. **서비스**: Supabase Auth 통합 → 핵심 CRUD 검증 및 에러 핸들링 개선 → Realtime/스토리지 기능 연결.
3. **아키텍처**: API Gateway 구현 및 보안 강화 → Supabase 스키마 배포 파이프라인 → 통합 테스트/모니터링 자동화.

## 6. 결론
- 디자인 측면에서는 체계적인 토큰·모션 시스템이 부재하여 기기별 경험 일관성이 떨어지므로 디자인 시스템화가 시급합니다.
- 서비스 계층은 Supabase SDK를 활용한 기초 CRUD만 구현된 상태로, 인증·역할·실시간 기능이 남아 있어 사용자 경험과 보안 측면의 리스크가 존재합니다.
- 아키텍처는 멀티 호스트 구조가 잘 문서화되어 있으나 서버 측 BFF와 자동화된 운영 전략이 미완성입니다. 단기적으로는 API Gateway와 CI/CD 내 Supabase 파이프라인 구축이 필요합니다.

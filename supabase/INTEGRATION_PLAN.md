# NexaCRM Supabase 연동 준비 플랜

## 1. 아키텍처 진단 요약
- **클라이언트 → 게이트웨이 → 마이크로서비스 레이어**: Blazor Web Client가 Ocelot API 게이트웨이를 거쳐 Contact/Deal/Identity 서비스로 라우팅하는 계층형 구조입니다.【F:README.md†L27-L68】
- **공유 빌딩 블록**: EventBus와 Common 라이브러리가 모든 서비스에서 공통 유틸리티 및 메시징 규약을 제공합니다.【F:README.md†L55-L90】
- **기본 데이터 스토어**: 현재 PostgreSQL과 RabbitMQ를 사용하는 구조로, Supabase PostgreSQL과 Realtime 기능이 동일 영역을 대체·확장할 수 있습니다.【F:README.md†L46-L63】

> 위 구조를 유지하면서 데이터 계층을 Supabase로 전환하고, 실시간/자동화 기능을 점진적으로 도입하는 것이 이번 준비 플랜의 목표입니다.

## 2. Supabase 도입 목표
1. **PostgreSQL 호스팅 이전**: 각 마이크로서비스가 사용하는 PostgreSQL 스키마를 Supabase 프로젝트로 이전합니다.
2. **통합 인증 전략**: Duende IdentityServer는 계속 액세스 토큰을 발급하되, 사용자 프로필/권한 데이터는 Supabase Auth 및 `profiles`/`organization_users` 테이블을 단일 소스로 사용합니다.
3. **실시간·자동화 활용**: 지원 티켓, 작업(Task), 알림 등 실시간 업데이트가 필요한 도메인에 Supabase Realtime 및 Edge Functions를 연결합니다.
4. **DevOps 일관성 확보**: Supabase CLI와 GitHub Actions(또는 기존 파이프라인)를 이용해 스키마/정책을 버전 관리합니다.

## 3. 서비스별 연동 전략
| 서비스 계층 | 주요 책임 | Supabase 연동 포인트 | 참고 코드 |
| --- | --- | --- | --- |
| **Identity.API** | OAuth2/JWT 발급, 사용자/역할 관리 | `auth.users`, `profiles`, `user_roles`, `organization_users`를 IdentityServer 스토어로 사용. 서비스 키(Service Role) 기반 서버-사이드 관리. | [`Services.Identity.API`](../src/Services/Identity.API/Services.Identity.API.csproj) |
| **Contact.API** | 고객/연락처 CRUD, 활동 로그 | `contacts`, `companies`, `activities`, `db_customers` 등 CRM 핵심 테이블을 Supabase PostgREST/RPC 또는 직접 Npgsql 연결로 조작. | [`IContactService`](../src/Web/NexaCRM.WebClient/Services/Interfaces/IContactService.cs) |
| **Deal.API** | 딜 파이프라인, 일정 | `deals`, `deal_stages`, `sales_appointments`, `consultation_notes` 등을 트랜잭션 단위로 사용. 단계 변경 시 `audit_logs` 트리거. | [`ISalesManagementService`](../src/Web/NexaCRM.WebClient/Services/Interfaces/ISalesManagementService.cs) |
| **Web Client (Blazor)** | UI, 그래프, 실시간 알림 | 인증 후 Supabase JS Client(SPA) 또는 API 게이트웨이를 통한 REST 호출. 실시간 피드/티켓은 Supabase Realtime 구독. | [`NexaCRM.WebClient`](../src/Web/NexaCRM.WebClient/Program.cs) |
| **EventBus** | 메시징, 이벤트 발행/구독 | 주요 상태 변경 시 Supabase `integration_events` 테이블을 소스로 사용하고, RabbitMQ와 동기화. 장기적으로는 Supabase Function → EventBus 브릿지를 구성. | [`BuildingBlocks.EventBus`](../src/BuildingBlocks/EventBus) |

### 3.1 연결 패턴
- **서버 측(마이크로서비스)**: 기존 Npgsql DbContext를 유지하면서 Supabase 호스트/인증서에 맞춰 연결 문자열을 교체합니다. 고급 기능이 필요하면 [Supabase .NET SDK](https://github.com/supabase-community/supabase-csharp)의 Admin 클라이언트를 EventBus와 함께 사용합니다.
- **클라이언트 측(Blazor)**: 인증 토큰 발급은 IdentityServer를 유지하되, 추가적인 실시간 데이터는 Supabase JS Client를 통해 직접 구독하거나 SignalR 대체로 Supabase Realtime을 사용하는 하이브리드 방식을 적용합니다.

## 4. 인프라 및 설정 준비
1. **환경 변수 통합**
   - `SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY`, `SUPABASE_ANON_KEY`: API 게이트웨이 및 각 서비스의 Secret Manager(Docker Secrets, Azure Key Vault 등)에 저장.
   - `SUPABASE_DB_CONNECTION`: Npgsql 기반 연결 문자열 (`Host=<supabase_host>;User Id=<user>;Password=<pw>;Database=postgres;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=50;Ssl Mode=Require;Trust Server Certificate=true`).
2. **구성 파일 템플릿**
   ```jsonc
   // appsettings.Supabase.json
   {
     "Supabase": {
       "Url": "https://<project>.supabase.co",
       "AnonKey": "<anon-key>",
       "ServiceRoleKey": "<service-key>",
       "Database": {
         "ConnectionString": "${SUPABASE_DB_CONNECTION}"
       },
       "Client": {
         "AutoRefreshToken": true,
         "AutoConnectRealtime": false
       }
     }
   }
   ```
   - 각 마이크로서비스 `Program.cs` 또는 `Startup`에서 `builder.Configuration.AddJsonFile("appsettings.Supabase.json", optional: true)` 후 환경 변수를 덮어씁니다.
   - Supabase 설정이 바인딩된 후 `BuildingBlocks.Common.Supabase`에서 제공하는 `services.AddSupabaseCore(builder.Configuration);`를 호출해 공통 `ISupabaseClientFactory`를 등록합니다.
3. **네트워크/보안**
   - Supabase 호스트를 VNet 또는 프라이빗 피어링(가능 시)으로 연결하여 서비스에서 직접 접근.
   - API 게이트웨이/서비스에서만 Service Role Key 사용, 클라이언트는 Anon Key로 제한.

## 5. 데이터 마이그레이션 로드맵
1. **스키마 정합성 확인**: `supabase/DB_ARCHITECTURE.md`의 테이블 정의와 현행 EF Core 모델을 대조하여 누락된 열/인덱스를 파악합니다.
2. **마이그레이션 스크립트 작성**: Supabase CLI의 `supabase db diff`를 사용해 `supabase/migrations/*.sql`을 생성하고 버전 관리합니다.
3. **데이터 이행 단계**
   - 1단계: 기존 PostgreSQL에서 Supabase로 read replica 구성 후 dump/restore.
   - 2단계: Identity 데이터(`auth.users`, `profiles`) → 애플리케이션 사용자 동기화.
   - 3단계: Deal/Contact/Task 등 도메인 데이터 점검 및 검증 스크립트 실행.
4. **롤백 계획**: 데이터 마이그레이션 시점에 스냅샷을 보관하고, 실패 시 기존 PostgreSQL로 커넥션 문자열을 되돌릴 수 있도록 IaC 파이프라인에 스위치 변수 추가.

## 6. API 게이트웨이 및 서비스 코드 변경 가이드
1. **Ocelot 라우팅 업데이트**
   - 게이트웨이에서 `/api/contact`, `/api/deal` 라우트를 그대로 유지하되, 내부적으로 Supabase 기반 서비스 인스턴스에 연결.
   - 필요 시 게이트웨이에 Supabase 토큰을 전달하기 위한 HTTP 헤더(`apikey`, `Authorization: Bearer <jwt>` ) 주입 로직 추가.
2. **서비스 레이어**
   - Repository/HttpClient를 Supabase REST Endpoint(PostgREST)로 전환하거나 EF Core `DbContext`의 연결 문자열을 Supabase로 교체.
   - 실시간 업데이트가 필요한 메서드는 Supabase Realtime 트리거를 구독해 캐시를 무효화하도록 EventBus 메시지를 발행.
3. **테스트 코드 갱신**
   - `tests/BlazorWebApp.Tests` 등 단위 테스트에서 Supabase Mock 서버(PostgREST docker image) 또는 WireMock을 이용한 계약 테스트를 준비.

## 7. 실시간 & 자동화 아키텍처
- **Realtime 채널**: `support_tickets`, `ticket_messages`, `tasks`, `notification_feed` 테이블에 대해 Supabase 리스너를 등록하고, 게이트웨이가 SignalR 허브 대신 SSE/WebSocket 프록시 역할을 수행.
- **Edge Functions / Cron**: `sms_schedules` 처리, `dedupe_runs` 배치, `audit_logs` 동기화 등을 Supabase Edge Function으로 구현 후 EventBus로 후속 작업을 발행.
- **감사/모니터링**: `audit_logs`, `integration_events`를 DataDog/Elastic으로 스트리밍하기 위해 Supabase Logical Replication을 활용.

## 8. 검증 및 관측 지표
- **핵심 지표**: API 응답 시간, Realtime 전파 지연, Edge Function 실패율, Supabase 연결 풀 사용량.
- **알림 구성**: Supabase Status 웹훅, Grafana 알람, PagerDuty 연계.
- **계약 테스트**: 서비스 인터페이스(`IContactService`, `ITaskService`, `ISupportTicketService` 등)에 대해 Supabase Staging 환경을 대상으로 스모크 테스트를 자동화.

## 9. 작업 백로그 (우선순위 순)
1. ✅ `supabase` 프로젝트 생성 및 환경 변수 등록
2. ✅ `supabase/migrations` 폴더 구조 확인 및 Git 버전 관리 설정 완료
3. 🔄 Identity 서비스용 사용자/역할 스키마 매핑 구현 (EF Core → Supabase)
4. 🔄 Contact/Deal 서비스의 Repository 계층에 Supabase 연결 문자열 적용
5. 🔄 실시간 통신(Push/Notification) 요구사항 정리 후 Supabase Realtime 프로토타입 작성
6. 🔄 CI 파이프라인에 Supabase CLI(`supabase db push`, `supabase db dump`) 통합
7. 🔜 운영 모니터링 대시보드에 Supabase 지표 추가 (Grafana/Prometheus)
8. 🔜 Supabase Edge Function으로 SMS 예약 발송 자동화 시나리오 구현

> 위 백로그는 실제 진척 상황에 맞춰 상태 값을 갱신하며, 완료 항목은 체크 표시로 관리합니다.

---
이 플랜 문서를 기준으로 스키마 정의서([`supabase/DB_ARCHITECTURE.md`](./DB_ARCHITECTURE.md))와 연계하여 개발·운영 팀이 동일한 기준으로 Supabase 전환을 추진할 수 있습니다.

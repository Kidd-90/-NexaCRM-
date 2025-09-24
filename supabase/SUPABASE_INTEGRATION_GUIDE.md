# NexaCRM Supabase 연동 가이드

## 1. 목적과 범위
- NexaCRM의 모든 핵심 CRM 데이터를 Supabase PostgreSQL로 이전하고, 인증·저장소·실시간 기능을 일관되게 활용하기 위한 기준을 제공합니다.
- 기존 문서화된 데이터 모델과 RLS 전략을 실 구현 단계에서 재사용할 수 있도록 개발 환경, 패키지, 코드 구조를 안내합니다.

## 2. Supabase 환경 준비
### 2.1 프로젝트 및 조직 설정
1. [Supabase 대시보드](https://supabase.com/dashboard)에 접속해 NexaCRM 전용 프로젝트를 생성합니다.
2. 조직 이름과 프로젝트 이름을 `nexacrm-{환경}` 형식으로 지정해 환경별 리소스를 명확히 구분합니다.
3. 리전은 CRM 사용자에게 가장 가까운 리전을 선택하고, 사용량 급증에 대비해 프로 비전드 데이터베이스 플랜을 검토합니다.

### 2.2 보안 키 및 환경 변수 관리
- Supabase에서 발급되는 `anon`/`service_role` 키는 운영/스테이징/개발 환경별로 분리해 저장합니다.
- NexaCRM 서버 구성 요소와 Blazor WebApp에 다음 환경 변수를 주입합니다.

| 변수 | 설명 | 권장 저장 위치 |
| --- | --- | --- |
| `SUPABASE_URL` | Supabase 프로젝트 URL | `.env` 또는 Azure Key Vault |
| `SUPABASE_ANON_KEY` | 클라이언트용 공개 키 | Blazor WebApp `appsettings.{Environment}.json` (User Secrets 권장) |
| `SUPABASE_SERVICE_ROLE_KEY` | 서버 전용 키 | API 백엔드 비밀 저장소 |
| `SUPABASE_JWT_SECRET` | 커스텀 JWT 검증용 시크릿 | API 게이트웨이 시크릿 매니저 |

### 2.3 데이터베이스 마이그레이션 적용
1. `supabase/migrations/schema.sql`과 `supabase/migrations/rls.sql`을 Supabase CLI로 순차 실행합니다.
2. CLI 사용 예시는 아래와 같습니다.
   ```bash
   supabase db push --file supabase/migrations/schema.sql
   supabase db push --file supabase/migrations/rls.sql
   ```
3. 마이그레이션 후 [Supabase DB 아키텍처 가이드](DB_ARCHITECTURE.md)를 참고해 각 도메인 테이블과 정책이 정상적으로 생성됐는지 확인합니다.

## 3. Blazor 및 .NET 연동 절차
### 3.1 NuGet 패키지 설치
프로젝트 루트에서 다음 명령으로 Supabase .NET SDK를 추가합니다.
```bash
dotnet add src/Web/NexaCRM.WebClient/NexaCRM.WebClient.csproj package Supabase
dotnet add src/Web/NexaCRM.WebClient/NexaCRM.WebClient.csproj package Postgrest
```

### 3.2 DI 구성
`Program.cs` 또는 서비스 등록 파일에 Supabase 클라이언트를 싱글턴으로 등록합니다.
```csharp
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true
    };

    return new Supabase.Client(
        config["Supabase:Url"],
        config["Supabase:AnonKey"],
        options
    );
});
```
> API 백엔드 서비스에서는 `SUPABASE_SERVICE_ROLE_KEY`를 사용해 행 단위 보안(RLS)을 우회하는 서버 사이드 호출을 구현합니다.

### 3.3 서비스 구현 패턴
- 각 도메인 서비스(Contact, Deal, Support 등)는 기존 인터페이스(`IContactService`, `ISupportTicketService` 등)를 유지한 채 Supabase 데이터 접근 코드로 교체합니다.
- 데이터 접근 레이어에서는 `Postgrest.Table<T>`를 사용해 CRUD 및 필터링 로직을 작성하고, `Realtime` 기능을 통해 실시간 업데이트를 구독합니다.
- 장기 실행 작업(예: 중복 데이터 정리, 예약 발송)은 Supabase Edge Functions 또는 Scheduled Functions로 이전합니다.

## 4. 배포 및 운영 체크리스트
- [ ] Supabase 프로젝트에 IP 접근 제어와 비밀번호 정책을 설정했습니다.
- [ ] 스토리지 버킷 구조(티켓 첨부, 캠페인 에셋 등)를 정의하고, 공개/비공개 권한을 검증했습니다.
- [ ] 백업 및 PITR(Point in Time Recovery) 정책을 검토해 데이터 복구 전략을 수립했습니다.
- [ ] 관제 대시보드에 Supabase 메트릭(쿼리 지연, 연결 수, 에러율)을 포함했습니다.

## 5. Supabase 연동 일정 및 진행 현황
### Phase 0 — 준비 작업 (완료)
- [x] Supabase 데이터 모델과 테넌트 RLS 전략을 문서화했습니다. (참고: `supabase/DB_ARCHITECTURE.md`)
- [x] 기본 스키마 및 RLS SQL 마이그레이션 초안을 저장소에 추가했습니다. (참고: `supabase/migrations/schema.sql`, `supabase/migrations/rls.sql`)

### Phase 1 — 인증 및 핵심 데이터 전환 (예상: 2024년 7월)
- [ ] Supabase Auth를 사용하도록 CustomAuthStateProvider를 교체하고 세션 갱신 로직을 구현합니다.
- [ ] 연락처·딜·업무 서비스의 CRUD 로직을 Supabase PostgREST 기반으로 마이그레이션합니다.
- [ ] 조직/역할 정보를 `organization_users`, `user_roles` 테이블과 연동해 RLS 정책과 연계합니다.

### Phase 2 — 실시간 기능 및 통합 (예상: 2024년 8월)
- [ ] 지원 티켓, 작업(Task), 알림(Notification) 서비스에 Supabase Realtime 구독을 연결합니다.
- [ ] SMS/이메일 예약 발송을 Supabase Edge Functions + Cron으로 이전합니다.
- [ ] 감사 로그(`audit_logs`)와 외부 연동 이벤트(`integration_events`)를 통해 메시지 브로커와 데이터 동기화를 확인합니다.

### Phase 3 — 분석 및 운영 고도화 (예상: 2024년 9월)
- [ ] 보고서·통계 테이블(`report_snapshots`, `statistics_daily`)에 대한 ETL 파이프라인을 구축합니다.
- [ ] SLA 모니터링과 장애 알림을 Supabase 모니터링 툴과 연동합니다.
- [ ] 장기 보관 정책(아카이브/파티셔닝)을 운영 환경에 적용합니다.

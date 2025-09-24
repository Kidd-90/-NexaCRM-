# Supabase Enterprise Services Overview

## Purpose
이 문서는 NexaCRM에서 Supabase 기반으로 구현된 핵심 엔터프라이즈 서비스(거버넌스, 개인화, 파일/커뮤니케이션 허브, 동기화)의 아키텍처와 사용 기술을 정리합니다. 향후 확장 시 참조용 베이스라인을 제공합니다.

## 사용 기술 요약
- **Supabase Auth & PostgREST**: 사용자 계정, 역할, 감사 로그, 보안 정책, 동기화 메타데이터 저장.
- **Supabase Storage**: `crm-documents` 버킷에 파일 업로드, 버전 관리, 스레드 메시지 첨부 지원.
- **Supabase Realtime (Event Streams)**: `IntegrationEventRecord` 테이블을 통해 이메일/SMS/푸시 등 비동기 처리 파이프라인과 연동.
- **Blazor WebAssembly (.NET 8)**: 모든 서비스는 `SupabaseClientProvider`를 통해 단일 커넥션을 재사용하며 비동기 API 패턴으로 설계됨.
- **Newtonsoft.Json**: 가변적인 메타데이터(위젯 설정, 기능 플래그, 커뮤니케이션 페이로드 등)를 직렬화.

## 서비스 개요
### 사용자/보안 거버넌스
- `SupabaseUserGovernanceService`는 `user_accounts`, `user_roles`, `security_policies`, `audit_logs` 테이블을 기반으로 사용자 생성/비활성화, 역할 부여, 비밀번호 재설정 토큰 발급, 감사 로그 조회를 수행합니다.
- 모든 변경 사항은 감사 로그에 기록되어 추적 가능성을 확보합니다.

### 설정·테마·대시보드 개인화
- `SupabaseSettingsCustomizationService`는 조직 및 사용자별 설정(`organization_settings`, `user_preferences`)을 관리하고, 대시보드 위젯/레이아웃(`dashboard_widgets`)과 KPI 스냅샷(`kpi_snapshots`)을 제공합니다.
- 위젯/기능 플래그는 JSON 기반으로 저장되어 유연한 확장이 가능합니다.

### 파일·커뮤니케이션 허브
- `SupabaseFileHubService`는 업로드 URL 발급, 파일 메타데이터 등록(`file_documents`), 버전 관리(`file_versions`)를 담당합니다.
- `communication_threads`, `thread_messages` 테이블을 통해 채널별 협업 스레드를 유지하고, 통합 이벤트를 생성하여 이메일/SMS/푸시 워커로 전달합니다.
- `SupabaseCommunicationHubService`는 이메일·SMS·푸시 요청을 이벤트 스트림에 큐잉하여 외부 채널과의 연동을 단순화합니다.

### 데이터 동기화 및 오프라인 전략
- `SupabaseSyncOrchestrationService`는 `sync_envelopes`, `sync_items`, `sync_conflicts` 테이블을 활용해 모바일/현장 단말의 오프라인 캐시와 충돌 해결 정책을 관리합니다.
- Sync 정책(`SyncPolicy`)에 따라 엔티티 범위와 새로고침 주기를 제어할 수 있으며, `sync.conflict.resolved` 이벤트로 후속 처리를 트리거합니다.

## 운영 권장 사항
1. **보안 키 관리**: 서비스 롤 키와 anon 키는 Azure Key Vault 등 비밀 저장소에서 로딩하고, RLS 정책으로 테이블 접근을 제한합니다.
2. **실시간 확장**: 향후 WebSocket 환경이 허용될 경우 `Supabase.Client`의 Realtime 구독을 활성화해 대시보드/커뮤니케이션 변화를 즉시 반영하십시오.
3. **모니터링**: `audit_logs` 및 `integration_events`는 운영 분석에 중요한 데이터이므로, Supabase Logflare 또는 BigQuery 연동을 고려합니다.
4. **백업 전략**: 파일 버킷과 Postgres 스키마에 대해 주기적인 스냅샷을 구성하여 규제 요구사항을 충족합니다.

## 다음 단계
- 관리자 UI에서 새로운 서비스 메서드를 호출하는 Form 및 Dashboard 컴포넌트를 추가합니다.
- Background Worker(예: Azure Functions, Supabase Edge Functions)를 통해 통합 이벤트를 처리하고 외부 채널과 실제 연동을 완료합니다.
- 모바일 앱/오프라인 모드 지원을 위해 `SyncEnvelope` 스키마와 충돌 정책을 세분화합니다.

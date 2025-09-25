# Supabase Enterprise Services Overview

이 문서는 NexaCRM WebClient에서 Supabase 기반 거버넌스, 파일/커뮤니케이션 허브, 개인화, 동기화 모듈을 어떻게 구성했는지 요약합니다.

## 공통 설계
- `SupabaseClientProvider`를 통해 Supabase 초기화를 단일 지점에서 수행하고, WebAssembly 환경에서 Realtime 미지원 시 graceful fallback을 제공합니다.
- 실제 Supabase 백엔드 연결이 어려운 개발 환경을 위해 `SupabaseEnterpriseDataStore`를 싱글톤으로 등록하여 인메모리 저장소를 제공합니다. 해당 저장소는 런타임 중 Supabase 테이블을 모사하며, 이후 실 서비스에 맞춰 PostgREST 호출로 교체할 수 있습니다.
- 모든 서비스는 `EnsureClientAsync` 도우미를 호출해 Supabase 초기화 시도 후, 실패 시 경고 로그를 남기고 인메모리 경로로 계속 동작합니다.

## 사용자/보안 거버넌스 (`IUserGovernanceService`)
- 사용자 생성, 역할 할당, 비활성화, 비밀번호 재설정 티켓, 보안 정책 저장/조회, 감사 로그 조회를 제공합니다.
- 감사 로그는 `SecurityAuditLogEntry` 모델로 관리하며, 조직 단위로 역순 정렬된 히스토리를 제공합니다.

## 개인화/대시보드 (`ISettingsCustomizationService`)
- 조직 기본 설정, 사용자 선호도, 대시보드 위젯 레이아웃, KPI 스냅샷을 CRUD 방식으로 지원합니다.
- 대시보드 위젯은 순서를 강제하기 위해 저장 시 자동으로 인덱싱합니다.

## 파일·커뮤니케이션 허브 (`IFileHubService`, `ICommunicationHubService`)
- 문서 등록과 버전 관리, 문서 별 스레드 생성, 멀티 채널 메시지 기록을 제공합니다.
- 일반 커뮤니케이션 스레드는 `ICommunicationHubService`로 분리해 파일 외 시나리오(영업/지원 등)에 재사용할 수 있습니다.

## 동기화 (`ISyncOrchestrationService`)
- 오프라인 envelope 등록, 미적용 envelope 조회, 적용 처리, 충돌 기록/조회 기능을 제공합니다.
- 충돌 정보는 envelope-organization 조합으로 그룹화되어 모바일/데스크톱 클라이언트가 쉽게 수집할 수 있습니다.

## 테스트 전략
- `SupabaseEnterpriseDataStore`는 순수 C# 인메모리 컬렉션을 사용하므로 단위 테스트에서 별도의 외부 의존성을 요구하지 않습니다.
- 서비스별 동작은 `NexaCRM.WebClient.UnitTests` 프로젝트에서 검증하며, 향후 실 Supabase 연동 시에는 통합 테스트를 추가할 수 있습니다.

# Supabase GitHub Actions 파이프라인

이 문서는 GitHub Actions를 사용해 Supabase 스키마를 검증하고 환경별로 배포하도록 구성한 `.github/workflows/supabase-schema.yml` 워크플로우의 구조와 사용 기술을 정리합니다.【F:.github/workflows/supabase-schema.yml†L1-L84】

## 1. 사용 기술 요약

| 기술 | 목적 | 참고 구성 |
| --- | --- | --- |
| GitHub Actions | CI/CD 파이프라인 오케스트레이션 | `jobs.validate`, `jobs.deploy-staging`, `jobs.deploy-production` |
| Supabase CLI (`supabase/setup-cli@v1`) | Supabase 데이터베이스 스키마 정적 분석, 리셋, 배포 | `supabase db lint`, `supabase db reset`, `supabase db push` |
| .NET 8 SDK (`actions/setup-dotnet@v4`) | 스키마 변경 이후 C# 계약 테스트 실행 | `dotnet test ./tests/BlazorWebApp.Tests` |

## 2. 워크플로우 트리거

- **Pull Request** (`main`, `develop`, `release/**`): 스키마 변경이 포함된 PR에서 자동으로 실행되어 lint → reset → push → 테스트 순으로 검증합니다.【F:.github/workflows/supabase-schema.yml†L5-L36】
- **Push** (`develop`, `release/**`, `main`): 브랜치 정책에 따라 Staging/Production 대상 Supabase 프로젝트로 스키마를 배포합니다.【F:.github/workflows/supabase-schema.yml†L12-L36】

## 3. Job 단계별 설명

### 3.1 `validate` Job (Pull Request)
1. **코드 체크아웃**: `actions/checkout@v4`로 저장소를 가져옵니다.
2. **Supabase CLI 설치**: `supabase/setup-cli@v1`로 최신 CLI를 설치합니다.
3. **.NET SDK 설치**: `actions/setup-dotnet@v4`로 .NET 8 환경을 준비합니다.
4. **의존성 복원**: `dotnet restore`로 NuGet 패키지를 다운로드합니다.
5. **스키마 Lint**: `supabase db lint`로 SQL 자산의 정적 분석을 수행합니다.
6. **Dev DB Reset**: `supabase db reset --force --non-interactive`로 개발용 데이터베이스를 초기화합니다.
7. **Dev DB Push**: `supabase db push`로 스키마와 RLS를 적용합니다.
8. **계약 테스트**: `dotnet test ./tests/BlazorWebApp.Tests --configuration Release --no-build`로 코드-스키마 일치성을 검증합니다.【F:.github/workflows/supabase-schema.yml†L38-L73】

### 3.2 `deploy-staging` Job (release 브랜치 Push)
- Staging 환경 보호를 위해 GitHub Environment `supabase-staging`을 사용합니다.
- `supabase db push --include "functions,policies"` 명령으로 스키마, 정책, 함수 변경을 적용합니다.【F:.github/workflows/supabase-schema.yml†L75-L95】

### 3.3 `deploy-production` Job (main 브랜치 Push)
1. **Dry Run**: `supabase db push --dry-run`으로 변경 사항을 사전 검토합니다.
2. **프로덕션 배포**: 승인 후 `supabase db push --include "functions,policies"`로 실제 배포를 실행합니다.
3. GitHub Environment `supabase-production`을 활용해 승인을 요구합니다.【F:.github/workflows/supabase-schema.yml†L97-L118】

## 4. 필요한 시크릿과 환경 변수

| 시크릿 이름 | 용도 |
| --- | --- |
| `SUPABASE_ACCESS_TOKEN` | Supabase CLI 인증 토큰 |
| `SUPABASE_DEV_DB_URL` | Dev 환경 데이터베이스 연결 문자열 |
| `SUPABASE_STAGING_DB_URL` | Staging 환경 데이터베이스 연결 문자열 |
| `SUPABASE_PROD_DB_URL` | Production 환경 데이터베이스 연결 문자열 |

> **주의**: `db reset` 명령은 지정된 데이터베이스를 초기화하므로, Dev 환경 전용 연결 문자열만 사용해야 합니다.

## 5. 확장 아이디어

- `supabase db diff` 결과를 아티팩트로 업로드해 리뷰어가 변경점을 쉽게 확인하도록 개선할 수 있습니다.
- Supabase Edge Function 테스트를 추가하려면 `supabase functions serve --env-file`과 같은 명령을 통합하는 별도 Job을 구성합니다.
- 실패 알림을 Slack/Teams로 전송하려면 GitHub Actions에서 Webhook 통합 단계를 추가하세요.

본 문서는 Supabase 관련 CI/CD 설정 변경 시 함께 업데이트하여 기술 스택과 운영 절차를 최신 상태로 유지합니다.

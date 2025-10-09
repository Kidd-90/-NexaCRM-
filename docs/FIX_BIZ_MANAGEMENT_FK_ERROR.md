# 비즈니스 관리 페이지 외래 키 오류 수정

## 문제 설명
회사 추가 시 다음과 같은 오류가 발생했습니다:
```
{"code":"23503","details":"Key (tenant_unit_id)=(1) is not present in table \"organization_units\".","hint":null,"message":"insert or update on table \"biz_companies\" violates foreign key constraint \"org_companies_tenant_unit_id_fkey\""}
```

이는 `biz_companies` 테이블의 `tenant_unit_id` 컬럼이 `organization_units` 테이블의 `id`를 참조하는데, 해당 ID가 존재하지 않아서 발생하는 외래 키 제약 조건 위반입니다.

## 수정 사항

### 1. 데이터베이스 수정 (필수)

**Supabase SQL Editor에서 다음 스크립트를 실행하세요:**

```sql
-- supabase/fix_organization_units.sql 파일 내용을 실행

-- 1. 현재 organization_units 확인
SELECT * FROM organization_units;

-- 2. 기본 조직 단위 삽입 (없는 경우)
INSERT INTO organization_units (id, name, tenant_code, created_at, updated_at)
VALUES (1, '기본 조직', 'DEFAULT', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- 3. 시퀀스 재설정
SELECT setval('organization_units_id_seq', COALESCE((SELECT MAX(id) FROM organization_units), 1), true);

-- 4. 확인
SELECT * FROM organization_units;
```

### 2. 코드 수정 사항

#### a. OrganizationUnitRecord 모델 추가
- **파일**: `src/NexaCRM.Service/Abstractions/Models/Supabase/OrganizationUnitRecord.cs`
- organization_units 테이블을 매핑하는 모델 추가

#### b. IBizManagementService 인터페이스 확장
- **파일**: `src/NexaCRM.Service/Abstractions/Interfaces/IBizManagementService.cs`
- 조직 단위 관련 메서드 추가:
  - `GetOrganizationUnitsAsync()`: 모든 조직 단위 조회
  - `GetOrganizationUnitByIdAsync(long id)`: 특정 조직 단위 조회
  - `GetOrCreateDefaultOrganizationUnitAsync()`: 기본 조직 단위 가져오기 또는 생성

#### c. BizManagementService 구현 추가
- **파일**: `src/NexaCRM.Service/Core/Admin/Services/BizManagementService.cs`
- 조직 단위 관련 메서드 구현
- 기본 조직 단위가 없으면 자동으로 생성하는 로직 추가

#### d. BizManagementPage.razor 개선
- **파일**: `src/NexaCRM.UI/Pages/BizManagementPage.razor`
- 하드코딩된 `tenantUnitId = 1` 대신 동적으로 가져오기
- 사용자 클레임에서 `tenant_unit_id` 조회
- 없으면 `GetOrCreateDefaultOrganizationUnitAsync()` 호출
- 에러 메시지 표시 UI 추가
- 외래 키 제약 조건 오류 시 사용자 친화적 메시지 표시

## 적용 방법

### 1단계: SQL 스크립트 실행 (필수)
Supabase Dashboard > SQL Editor에서 `supabase/fix_organization_units.sql` 파일을 실행하세요.

### 2단계: 애플리케이션 빌드
```cmd
cd src\NexaCRM.WebServer
dotnet build
```

### 3단계: 애플리케이션 실행 및 테스트
```cmd
dotnet run
```

1. 로그인 후 "비즈니스/프랜차이즈 관리" 페이지 접속
2. "회사 추가" 버튼 클릭
3. 회사 정보 입력 후 저장
4. 오류 없이 정상적으로 저장되는지 확인

## 예상 결과

### 성공 케이스
- 회사가 정상적으로 추가됩니다
- "새 회사가 추가되었습니다" 메시지가 표시됩니다
- 회사 목록에 새로 추가한 회사가 나타납니다

### 실패 케이스 (개선된 에러 메시지)
- **조직 단위 없음**: "조직 단위(tenant_unit_id=1)가 존재하지 않습니다. 데이터베이스 관리자에게 문의하세요."
- **중복 코드/이름**: "중복된 코드 또는 회사명입니다."
- **기타 오류**: 상세한 오류 메시지 표시

## 주의사항

1. **SQL 스크립트 실행 필수**: 코드 수정만으로는 문제가 해결되지 않습니다. 반드시 SQL 스크립트를 먼저 실행해야 합니다.

2. **프로덕션 환경**: 프로덕션 환경에서는 마이그레이션 스크립트로 관리하는 것이 좋습니다.

3. **다중 테넌트**: 여러 조직을 관리하는 경우, 각 테넌트에 맞는 `organization_units` 레코드가 있는지 확인하세요.

## 추가 개선 사항

- 사용자 로그인 시 `tenant_unit_id`를 JWT 클레임에 포함하도록 인증 로직 개선
- 관리자 페이지에서 조직 단위를 관리할 수 있는 UI 추가
- RLS(Row Level Security) 정책으로 조직 단위별 데이터 격리 강화

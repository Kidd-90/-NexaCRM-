# 팀 관리 기능 추가

## 개요
비즈니스 관리 페이지에 **팀 관리** 기능을 추가했습니다. 이제 회사, 지점과 함께 팀을 통합 관리할 수 있습니다.

## 추가된 기능

### 1. 팀 관리 탭
- 회사 관리, 지점 관리와 함께 **팀 관리** 탭 추가
- 팀 목록 조회, 필터링, 검색 기능
- 회사별, 지점별, 상태별 필터링 지원

### 2. 팀 CRUD 기능
- ✅ **팀 추가**: 새로운 팀 생성
- ✅ **팀 편집**: 기존 팀 정보 수정
- ✅ **팀 삭제**: 팀 삭제 (확인 대화상자 포함)
- ✅ **팀 조회**: 조직 단위별 팀 목록 조회

### 3. 팀 속성
각 팀은 다음 정보를 관리할 수 있습니다:
- **기본 정보**
  - 코드 (필수): 팀 고유 코드 (예: TEAM001)
  - 팀명 (필수): 팀 이름 (예: 영업1팀)
  - 상태: 활성/비활성
  
- **조직 구조**
  - 회사: 소속 회사 (선택사항)
  - 지점: 소속 지점 (선택사항)
  - 조직 단위: 테넌트 단위 (자동 설정)
  
- **팀 관리**
  - 매니저명: 팀 매니저 이름
  - 멤버수: 팀 멤버 수 (자동 계산 또는 수동 입력)
  - 등록일: 팀 생성일

### 4. 통계 대시보드 확장
통계 탭에 팀 관련 통계 추가:
- 전체 팀 수
- 활성 팀 수
- 전체 멤버 수 (모든 팀의 멤버 수 합계)

## 데이터베이스 구조

### teams 테이블
```sql
CREATE TABLE teams (
  id BIGSERIAL PRIMARY KEY,
  tenant_unit_id BIGINT NOT NULL REFERENCES organization_units(id) ON DELETE CASCADE,
  company_id BIGINT REFERENCES biz_companies(id) ON DELETE SET NULL,
  branch_id BIGINT REFERENCES biz_branches(id) ON DELETE SET NULL,
  code TEXT NOT NULL UNIQUE,
  name TEXT NOT NULL,
  manager_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  manager_cuid TEXT REFERENCES user_infos(user_cuid) ON DELETE SET NULL,
  manager_name TEXT,
  member_count INT NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  registered_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);
```

## 코드 변경 사항

### 1. TeamRecord 모델 업데이트
**파일**: `src/NexaCRM.Service/Abstractions/Models/Supabase/TeamRecord.cs`

기존 모델에 누락된 필드 추가:
- `TenantUnitId`: 조직 단위 ID
- `CompanyId`: 회사 ID (nullable)
- `BranchId`: 지점 ID (nullable)
- `ManagerId`: 매니저 사용자 ID (nullable)
- `ManagerCuid`: 매니저 사용자 CUID (nullable)

### 2. IBizManagementService 확장
**파일**: `src/NexaCRM.Service/Abstractions/Interfaces/IBizManagementService.cs`

팀 관리 메서드 추가:
```csharp
// Teams
Task<List<TeamRecord>> GetTeamsAsync(long tenantUnitId);
Task<List<TeamRecord>> GetTeamsByBranchAsync(long branchId);
Task<TeamRecord?> GetTeamByIdAsync(long id);
Task<TeamRecord> CreateTeamAsync(TeamRecord team);
Task<bool> UpdateTeamAsync(TeamRecord team);
Task<bool> DeleteTeamAsync(long id);
```

### 3. BizManagementService 구현
**파일**: `src/NexaCRM.Service/Core/Admin/Services/BizManagementService.cs`

팀 관련 CRUD 메서드 구현:
- Supabase 클라이언트를 통한 팀 데이터 CRUD 작업
- 에러 핸들링 및 로깅
- 조직 단위별, 지점별 팀 조회 지원

### 4. BizManagementPage.razor UI 추가
**파일**: `src/NexaCRM.UI/Pages/BizManagementPage.razor`

주요 변경 사항:
- **Tab enum 확장**: `Teams` 탭 추가
- **상태 변수 추가**:
  - `teams`, `filteredTeams`: 팀 데이터
  - `filterBranchId`: 지점 필터
  - `showTeamModal`, `currentTeam`: 팀 모달 관련
  
- **UI 컴포넌트**:
  - 팀 관리 탭 버튼
  - 팀 목록 테이블 (회사, 지점, 코드, 팀명, 매니저, 멤버수, 상태, 등록일)
  - 팀 추가/편집 모달
  - 회사 및 지점 드롭다운 (연동 필터링)
  
- **메서드 추가**:
  - `LoadTeams()`: 팀 데이터 로드
  - `ShowAddTeamModal()`, `EditTeam()`: 모달 표시
  - `SaveTeam()`, `DeleteTeam()`: 팀 CRUD
  - `OnTeamCompanyChanged()`: 회사 변경 시 지점 초기화
  - `GetBranchName()`: 지점 이름 조회 헬퍼

## 사용 방법

### 1. 팀 추가
1. "팀 관리" 탭 선택
2. "팀 추가" 버튼 클릭
3. 필수 정보 입력:
   - 코드 (예: TEAM001)
   - 팀명 (예: 영업1팀)
4. 선택 정보 입력:
   - 회사 선택
   - 지점 선택 (회사 선택 시 해당 회사의 지점만 표시)
   - 매니저명
5. "저장" 클릭

### 2. 팀 편집
1. 팀 목록에서 편집할 팀의 "편집" 버튼 (연필 아이콘) 클릭
2. 정보 수정
3. "저장" 클릭

### 3. 팀 삭제
1. 팀 목록에서 삭제할 팀의 "삭제" 버튼 (휴지통 아이콘) 클릭
2. 확인 대화상자에서 "확인" 클릭

### 4. 팀 필터링
좌측 필터 패널에서:
- **회사**: 특정 회사의 팀만 조회
- **지점**: 특정 지점의 팀만 조회 (회사 필터와 연동)
- **상태**: 활성/비활성 팀 필터링
- **검색**: 팀명 또는 코드로 검색

## 계층 구조

```
조직 단위 (Organization Unit)
  └─ 회사 (Company)
      └─ 지점 (Branch)
          └─ 팀 (Team)
              └─ 팀 멤버 (Team Members)
```

## 통계 정보

통계 탭에서 다음 정보를 확인할 수 있습니다:
- 📊 전체 회사 수 / 활성 회사 수
- 📊 전체 지점 수 / 활성 지점 수
- 📊 전체 팀 수 / 활성 팀 수
- 📊 전체 멤버 수 (모든 팀의 멤버 합계)

## 주의사항

1. **조직 단위 필수**: 팀을 추가하려면 유효한 `organization_units` 레코드가 있어야 합니다.
   - 이전에 생성한 `fix_organization_units.sql` 스크립트 실행 필요

2. **코드 고유성**: 팀 코드는 고유해야 합니다 (UNIQUE 제약 조건).

3. **회사-지점 관계**: 지점을 선택하려면 먼저 해당 지점의 회사를 선택해야 합니다.

4. **삭제 주의**: 팀을 삭제하면 관련된 팀 멤버 데이터도 함께 삭제될 수 있습니다 (CASCADE).

## 향후 개선 사항

- [ ] 팀 멤버 관리 기능 추가
- [ ] 매니저 사용자 선택 드롭다운 (user_infos 연동)
- [ ] 팀 성과 및 통계 대시보드
- [ ] 팀 간 멤버 이동 기능
- [ ] 팀 조직도 시각화
- [ ] CSV/Excel 내보내기 기능 구현

# 지점 및 팀 매니저 선택 기능 추가

## 개요
지점 추가/편집 및 팀 추가/편집 모달에 **매니저 선택** 기능을 추가했습니다. 이제 드롭다운 목록에서 매니저를 선택하여 지점 또는 팀의 관리자를 지정할 수 있습니다.

## 주요 변경 사항

### 1. 서비스 레이어 확장

#### IBizManagementService 인터페이스
**파일**: `src/NexaCRM.Service/Abstractions/Interfaces/IBizManagementService.cs`

사용자 목록 조회 메서드 추가:
```csharp
// Users
Task<List<UserDirectoryEntryRecord>> GetUsersAsync(long tenantUnitId);
```

#### BizManagementService 구현
**파일**: `src/NexaCRM.Service/Core/Admin/Services/BizManagementService.cs`

활성 사용자 목록 조회 구현:
```csharp
public async Task<List<UserDirectoryEntryRecord>> GetUsersAsync(long tenantUnitId)
{
    // tenant_unit_id로 필터링하고 status가 'active'인 사용자만 조회
    // job_title로 정렬하여 반환
}
```

### 2. UI 개선

#### 지점 모달 - 매니저 선택 추가
**위치**: 지점 추가/편집 모달

추가된 필드:
- **매니저 드롭다운**: 조직 내 활성 사용자 목록에서 선택
- **표시 형식**: `직책 (사번)` 또는 `user_cuid`
- **선택사항**: 매니저 미선택 가능
- **안내 메시지**: "매니저를 선택하면 지점 관리 권한이 부여됩니다."

#### 팀 모달 - 매니저 선택 개선
**위치**: 팀 추가/편집 모달

변경 사항:
- 기존: 텍스트 입력 (매니저명)
- 개선: 드롭다운 선택 (사용자 목록에서 선택)
- **자동 업데이트**: 매니저 선택 시 `ManagerId`, `ManagerCuid`, `ManagerName` 자동 설정

#### 목록 화면 개선
**지점 목록**:
- 매니저 컬럼에 `user_cuid` 대신 **직책 (사번)** 형식으로 표시
- 매니저 미지정 시 "-" 표시

**팀 목록**:
- 매니저 컬럼에 `manager_name` 대신 **직책 (사번)** 형식으로 표시
- 매니저 미지정 시 "-" 표시

### 3. 데이터 처리

#### 사용자 목록 로드
```csharp
private async Task LoadUsers()
{
    // 조직 단위별 활성 사용자 목록 조회
    // 실패 시 빈 목록 반환 (페이지 전체 실패 방지)
}
```

#### 매니저 정보 업데이트
**지점 저장 시**:
```csharp
if (!string.IsNullOrEmpty(currentBranch.ManagerCuid))
{
    // ManagerCuid로 사용자 찾기
    // ManagerId 자동 설정
}
```

**팀 저장 시**:
```csharp
private void OnTeamManagerChanged()
{
    // 매니저 선택 시 ManagerId, ManagerCuid, ManagerName 자동 설정
}
```

#### 매니저 표시 헬퍼
```csharp
private string GetManagerDisplayName(string? managerCuid)
{
    // user_cuid로 사용자 찾기
    // 직책 + 사번 형식으로 반환
    // 없으면 "-" 또는 user_cuid 반환
}
```

## 데이터 구조

### user_directory_entries 테이블
```sql
CREATE TABLE user_directory_entries (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL,
  user_cuid TEXT NOT NULL,
  tenant_unit_id BIGINT,
  job_title TEXT,              -- 직책
  employee_number TEXT,         -- 사번
  employment_type TEXT,         -- 고용 형태
  status TEXT DEFAULT 'active', -- 상태
  ...
);
```

### biz_branches 테이블
```sql
CREATE TABLE biz_branches (
  ...
  manager_id UUID REFERENCES auth.users(id),
  manager_cuid TEXT REFERENCES user_infos(user_cuid),
  ...
);
```

### teams 테이블
```sql
CREATE TABLE teams (
  ...
  manager_id UUID REFERENCES auth.users(id),
  manager_cuid TEXT REFERENCES user_infos(user_cuid),
  manager_name TEXT,
  ...
);
```

## 사용 방법

### 지점 매니저 지정

1. **지점 추가 또는 편집**
   - "지점 관리" 탭에서 "지점 추가" 또는 기존 지점의 "편집" 클릭

2. **매니저 선택**
   - "매니저" 드롭다운에서 지점 관리자 선택
   - 표시 형식: `영업부장 (EMP001)`, `개발팀장 (EMP002)` 등
   - 선택 안 함도 가능

3. **저장**
   - "저장" 클릭
   - 선택한 매니저 정보가 자동으로 `manager_id`, `manager_cuid`에 저장됨

### 팀 매니저 지정

1. **팀 추가 또는 편집**
   - "팀 관리" 탭에서 "팀 추가" 또는 기존 팀의 "편집" 클릭

2. **매니저 선택**
   - "매니저" 드롭다운에서 팀 관리자 선택
   - 선택 시 `ManagerId`, `ManagerCuid`, `ManagerName`이 자동 설정됨

3. **저장**
   - "저장" 클릭

### 매니저 정보 확인

**지점 목록**:
```
| 회사 | 코드 | 지점명 | 연락처 | 이메일 | 매니저 | 상태 |
|------|------|--------|--------|--------|--------|------|
| 본사 | BR001| 강남지점| 02-xxx | ...    | 영업부장 (EMP001) | 활성 |
```

**팀 목록**:
```
| 회사 | 지점 | 코드 | 팀명 | 매니저 | 멤버수 | 상태 |
|------|------|------|------|--------|--------|------|
| 본사 | 강남지점 | TEAM001 | 영업1팀 | 팀장 (EMP005) | 5명 | 활성 |
```

## 표시 우선순위

매니저 정보 표시 시 다음 우선순위로 처리:
1. `직책 (사번)` - 둘 다 있는 경우
2. `직책` - 직책만 있는 경우
3. `user_cuid` - 직책 없는 경우
4. `-` - 매니저 미지정

예시:
- `영업부장 (EMP001)` ✅
- `영업부장` ✅
- `cuid_user_123` ⚠️
- `-` (매니저 없음)

## 권한 관리

### 매니저 권한
매니저로 지정된 사용자는 다음 권한을 가질 수 있습니다:
- 지점 매니저: 해당 지점의 데이터 관리
- 팀 매니저: 해당 팀의 멤버 관리 및 데이터 관리

### RLS (Row Level Security)
데이터베이스 레벨에서 `manager_cuid`를 기반으로 접근 제어 가능:
```sql
CREATE POLICY manager_access ON biz_branches
  FOR ALL
  USING (manager_cuid = current_user_cuid());
```

## 주의사항

1. **사용자 데이터 필수**: 매니저 선택 기능을 사용하려면 `user_directory_entries` 테이블에 사용자 데이터가 있어야 합니다.

2. **활성 사용자만**: `status = 'active'`인 사용자만 매니저로 선택 가능합니다.

3. **데이터 정합성**: 
   - `ManagerId`와 `ManagerCuid`가 함께 저장됩니다
   - 팀의 경우 `ManagerName`도 함께 저장되어 빠른 조회가 가능합니다

4. **사용자 삭제**: 매니저로 지정된 사용자 삭제 시:
   - `ON DELETE SET NULL` 정책으로 manager_id가 NULL이 됩니다
   - manager_cuid도 NULL이 됩니다

## 테스트 시나리오

### 1. 매니저 없는 지점 생성
- [ ] 매니저 선택 없이 지점 생성 가능
- [ ] 목록에 "-" 표시 확인

### 2. 매니저 있는 지점 생성
- [ ] 드롭다운에서 매니저 선택
- [ ] 저장 후 목록에 매니저 정보 표시 확인
- [ ] `직책 (사번)` 형식으로 표시 확인

### 3. 지점 매니저 변경
- [ ] 기존 지점 편집
- [ ] 매니저 변경
- [ ] 저장 후 변경사항 반영 확인

### 4. 팀 매니저 선택
- [ ] 팀 추가 시 매니저 선택
- [ ] 매니저 정보가 자동으로 ManagerId, ManagerCuid, ManagerName에 설정되는지 확인

### 5. 사용자 데이터 없을 때
- [ ] user_directory_entries에 데이터가 없을 때
- [ ] 드롭다운이 비어있지만 페이지는 정상 작동
- [ ] "매니저 선택 (선택사항)" 옵션만 표시

## 향후 개선 사항

- [ ] 사용자 검색 기능 (드롭다운에 많은 사용자가 있을 때)
- [ ] 매니저 프로필 정보 툴팁 표시
- [ ] 매니저 연락처 빠른 보기
- [ ] 매니저별 담당 지점/팀 목록 보기
- [ ] 매니저 권한 설정 UI
- [ ] user_infos 테이블과 join하여 full_name 직접 표시

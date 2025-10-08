# 🚨 Admin Role이 없어서 로그인 루프 문제 해결

## 문제 증상

로그인 성공 후 대시보드와 로그인 페이지를 왔다 갔다 함 (인증 루프)

**로그 확인:**
```
REQ_USER GET /main-dashboard Auth=False Name=(none) Roles=
```

➡️ `Roles=` 가 비어있음! 이것이 문제의 원인입니다.

---

## 🔍 원인 진단

1. **Supabase Dashboard SQL Editor**에서 다음 스크립트 실행:

```sql
-- scripts/check-admin-roles.sql 내용 복사하여 실행
```

특히 마지막 `user_account_overview` 뷰 결과를 확인하세요:

```sql
SELECT 
    email,
    username,
    role_codes,  -- 이게 {} 또는 NULL이면 문제!
    array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';
```

**문제:**
- `role_codes` 컬럼이 `{}` (빈 배열) 또는 `NULL`
- `role_count`가 `0` 또는 `NULL`

---

## ✅ 해결 방법

### 방법 1: Admin Role만 빠르게 추가 (권장)

이미 Admin 계정이 있는 경우, Role만 추가하면 됩니다.

1. **Supabase Dashboard** → **SQL Editor**
2. **`scripts/add-admin-role.sql`** 내용 복사
3. **Run** 버튼 클릭

**예상 출력:**
```
📋 Found admin account: CUID=admin_xxxxx, Auth ID=f57ab878-...
✅ Added new Admin role
🎉 Admin role added successfully!

Verification:
| section           | email          | username | role_codes | role_count |
|-------------------|----------------|----------|------------|------------|
| === Verification === | admin@nexa.test | admin    | {Admin}    | 1          |
```

### 방법 2: 전체 Admin 계정 재생성

Admin 계정 자체가 없거나 문제가 있는 경우:

1. **먼저 Auth 사용자 생성** (없는 경우만)
   - Dashboard → **Authentication** → **Users** → **Add User**
   - Email: `admin@nexa.test`
   - Password: `admin123`
   - ☑️ Auto Confirm User

2. **SQL 실행**
   - Dashboard → **SQL Editor**
   - **`scripts/create-admin-complete.sql`** 내용 복사
   - **Run** 버튼 클릭

---

## 🧪 확인 및 테스트

### 1. DB에서 확인

```sql
SELECT 
    email,
    username,
    full_name,
    status,
    role_codes
FROM user_account_overview
WHERE email = 'admin@nexa.test';
```

**성공 시:**
```
| email          | username | full_name               | status | role_codes |
|----------------|----------|-------------------------|--------|------------|
| admin@nexa.test | admin    | System Administrator    | active | {Admin}    |
```

### 2. 서버 로그 확인

서버를 재시작하거나 다시 로그인하면 로그에 다음과 같이 표시되어야 합니다:

```
REQ_USER GET /main-dashboard Auth=True Name=admin@nexa.test Roles=Admin
```

✅ `Auth=True` 그리고 `Roles=Admin`이 나타나면 성공!

### 3. 브라우저에서 로그인 테스트

1. http://localhost:5065/login 접속
2. Username: `admin` (또는 `admin@nexa.test`)
3. Password: `admin123`
4. ✅ 대시보드로 바로 이동되고 머물러야 함

---

## 📊 Role 관련 테이블 구조

```
app_users (기본 계정 정보)
    ↓ user_cuid
user_roles (역할 할당) ← role_definitions (역할 정의)
    ↓
user_account_overview (뷰: 모든 정보 통합)
    → role_codes 컬럼 (배열)
```

**중요:** 
- `user_roles` 테이블에 `(user_cuid, role_code)` 레코드가 있어야 함
- `role_definitions`에 `role_code = 'Admin'` 레코드가 있어야 함
- `user_account_overview` 뷰가 이들을 JOIN하여 `role_codes` 배열 생성

---

## 🛠️ 문제가 계속되는 경우

### Option 1: user_roles 테이블 직접 확인

```sql
SELECT * FROM user_roles 
WHERE user_cuid IN (
    SELECT cuid FROM app_users WHERE email = 'admin@nexa.test'
);
```

비어있으면 `add-admin-role.sql` 실행

### Option 2: 뷰 재생성

```sql
DROP VIEW IF EXISTS user_account_overview;

CREATE VIEW user_account_overview AS
SELECT
  au.cuid,
  au.auth_user_id,
  au.email,
  au.status,
  au.created_at AS account_created_at,
  au.updated_at AS account_updated_at,
  ui.username,
  ui.full_name,
  ui.password_hash,
  ui.department,
  ui.job_title,
  ui.phone_number,
  ui.created_at AS profile_created_at,
  ui.updated_at AS profile_updated_at,
  COALESCE(
    ARRAY_AGG(ur.role_code ORDER BY ur.role_code)
      FILTER (WHERE ur.role_code IS NOT NULL),
    ARRAY[]::TEXT[]
  ) AS role_codes
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
GROUP BY
  au.cuid,
  au.auth_user_id,
  au.email,
  au.status,
  au.created_at,
  au.updated_at,
  ui.username,
  ui.full_name,
  ui.password_hash,
  ui.department,
  ui.job_title,
  ui.phone_number,
  ui.created_at,
  ui.updated_at;
```

---

## 💡 왜 이런 문제가 발생했나요?

1. **Auth 사용자만 생성됨**: Supabase Authentication에 사용자는 있지만
2. **App 레벨 데이터 누락**: `user_roles` 테이블에 역할이 할당되지 않음
3. **인증 vs 권한**: 인증(Authentication)은 성공했지만, 권한(Authorization) 정보가 없어서 페이지 접근이 거부됨

**ASP.NET Core Blazor의 동작:**
```csharp
// AuthenticationStateProvider.cs
foreach (var role in account.RoleCodes ?? Array.Empty<string>())
{
    if (!string.IsNullOrWhiteSpace(role))
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }
}
```

➡️ `RoleCodes`가 빈 배열이면 `ClaimTypes.Role` claim이 없음  
➡️ `@attribute [Authorize]` 페이지가 접근 거부  
➡️ 자동으로 `/login`으로 리디렉션  
➡️ 무한 루프 발생

---

## 🎉 최종 확인

모든 것이 정상이면:

```sql
SELECT 
    email,
    username, 
    status,
    role_codes,
    array_length(role_codes, 1) as role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';
```

**결과:**
```
email          | username | status | role_codes | role_count
---------------|----------|--------|------------|------------
admin@nexa.test| admin    | active | {Admin}    | 1
```

✅ `role_codes = {Admin}` 그리고 `role_count = 1`

이제 로그인하면 정상적으로 대시보드에 머물게 됩니다! 🎊

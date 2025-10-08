# 🚀 Admin 계정 완전 생성 가이드

## 📋 사전 준비사항

Supabase Dashboard에서 Auth 사용자를 먼저 생성해야 합니다.

## ✨ 1단계: Supabase Auth 사용자 생성

1. **Supabase Dashboard** 접속
   ```
   https://supabase.com/dashboard/project/YOUR_PROJECT_ID
   ```

2. 왼쪽 메뉴 **Authentication** → **Users** 클릭

3. **Add User** 버튼 클릭

4. 정보 입력:
   ```
   Email: admin@nexa.test
   Password: admin123
   ☑️ Auto Confirm User (반드시 체크!)
   ☐ Send Magic Link (체크 해제)
   ```

5. **Create User** 버튼 클릭

6. ✅ 생성 완료! (User ID는 자동으로 생성됨)

---

## 🎯 2단계: SQL 실행으로 완전한 계정 생성

1. Supabase Dashboard에서 **SQL Editor** 클릭

2. **New Query** 버튼 클릭

3. `/scripts/create-admin-complete.sql` 파일 내용 **전체 복사**

4. SQL Editor에 **붙여넣기**

5. **Run** 버튼 클릭 (또는 Cmd/Ctrl + Enter)

## 📊 예상 결과

성공하면 다음과 같은 메시지가 표시됩니다:

```
✅ Added/Updated role definitions: Admin, Manager, Sales, User
📋 Found auth user: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
✅ Created app_users record with CUID: admin_xxxxx
✅ Created/Updated user_infos record
✅ Created/Updated profiles record
✅ Created organization_users record
✅ Added Admin role to user_roles table

🎉 Admin account successfully created and linked!

📝 Login credentials:
   Email/Username: admin@nexa.test or admin
   Password: admin123

🔑 Auth User ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
🆔 CUID: admin_xxxxxxxxxxxxxxxxxxxxx
```

그리고 두 개의 확인 쿼리 결과가 표시됩니다:

### FINAL VERIFICATION 결과:
| email | auth_user_id | app_status | username | full_name | user_role | org_role | assigned_roles |
|-------|--------------|------------|----------|-----------|-----------|----------|----------------|
| admin@nexa.test | UUID | active | admin | System Administrator | Admin | Admin | {Admin} |

### VIEW VERIFICATION 결과:
| cuid | auth_user_id | email | username | full_name | status | role_codes |
|------|--------------|-------|----------|-----------|--------|------------|
| admin_xxx | UUID | admin@nexa.test | admin | System Administrator | active | {Admin} |

---

## 🔐 3단계: 로그인 테스트

### 서버 실행
```bash
cd src/NexaCRM.WebServer
dotnet run
```

### 브라우저에서 로그인
```
http://localhost:5000/login

Username: admin (또는 admin@nexa.test)
Password: admin123
```

---

## 🔧 문제 해결

### ❌ "Auth user not found" 오류
**원인**: Supabase Authentication에 사용자가 없음

**해결**: 
- 1단계를 다시 수행
- Dashboard > Authentication > Users에서 `admin@nexa.test`가 있는지 확인

### ❌ "role_code not found" 오류
**원인**: role_definitions 테이블이 비어있음

**해결**: 
- SQL 스크립트가 자동으로 역할을 추가하므로, 전체 스크립트를 다시 실행

### ❌ "already exists" 오류
**원인**: 이미 계정이 존재함

**해결**: 기존 데이터 확인 쿼리
```sql
SELECT * FROM app_users WHERE email = 'admin@nexa.test';
SELECT * FROM user_infos WHERE username = 'admin';
```

기존 계정 삭제 (필요시):
```sql
DO $$
DECLARE
    v_cuid TEXT;
BEGIN
    SELECT cuid INTO v_cuid FROM app_users WHERE email = 'admin@nexa.test';
    
    DELETE FROM user_roles WHERE user_cuid = v_cuid;
    DELETE FROM organization_users WHERE user_cuid = v_cuid;
    DELETE FROM profiles WHERE user_cuid = v_cuid;
    DELETE FROM user_infos WHERE user_cuid = v_cuid;
    DELETE FROM app_users WHERE cuid = v_cuid;
    
    RAISE NOTICE 'Deleted all admin account records';
END $$;
```

### 🔍 계정 상태 확인
```sql
-- 전체 상태 확인
SELECT 
    'app_users' AS table_name,
    au.email,
    au.auth_user_id,
    au.status
FROM app_users au
WHERE au.email = 'admin@nexa.test'

UNION ALL

SELECT 
    'auth.users' AS table_name,
    u.email,
    u.id::uuid,
    u.email_confirmed_at::text
FROM auth.users u
WHERE u.email = 'admin@nexa.test';
```

---

## 📝 생성되는 테이블 데이터

이 스크립트는 다음 테이블에 데이터를 생성/업데이트합니다:

1. ✅ `role_definitions` - Admin, Manager, Sales, User 역할 정의
2. ✅ `app_users` - 애플리케이션 사용자 기본 정보
3. ✅ `user_infos` - 사용자 상세 정보 (username, full_name 등)
4. ✅ `profiles` - Supabase Auth와 연결된 프로필
5. ✅ `organization_users` - 조직 멤버십 정보
6. ✅ `user_roles` - 사용자 역할 할당 (Admin)

---

## 🎉 완료!

이제 `admin@nexa.test` / `admin123`으로 로그인할 수 있습니다!

추가 계정이 필요하면:
1. Authentication > Users에서 새 사용자 생성
2. 회원가입 페이지에서 같은 이메일로 가입
3. 또는 이 스크립트를 수정해서 사용

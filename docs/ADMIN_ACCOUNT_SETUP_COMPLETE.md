# 🚀 Admin 계정 생성 - 완벽 가이드

## 📋 사전 준비

### 1단계: Supabase Dashboard에서 Auth 사용자 생성

1. **Supabase Dashboard** 접속: https://supabase.com/dashboard
2. 프로젝트 선택
3. 왼쪽 메뉴에서 **Authentication** 클릭
4. **Users** 탭 클릭
5. **Add User** 버튼 클릭
6. 다음 정보 입력:
   ```
   Email: admin@nexa.test
   Password: admin123
   ☑️ Auto Confirm User (반드시 체크!)
   ```
7. **Create User** 클릭

✅ Auth 사용자가 생성되었습니다!

---

## 💻 2단계: SQL 실행으로 모든 테이블 연결

1. Supabase Dashboard > **SQL Editor** 클릭
2. **New Query** 버튼 클릭
3. 아래 SQL을 **복사해서 붙여넣기**:

```sql
-- ============================================
-- Complete Admin Account Creation & Auth Linking
-- ============================================

-- Step 1: 테이블 제약조건 수정 (nullable로 변경)
ALTER TABLE app_users 
ALTER COLUMN auth_user_id DROP NOT NULL;

ALTER TABLE organization_users 
ALTER COLUMN user_id DROP NOT NULL;

-- Step 2: Admin 계정을 Supabase Auth와 연결
DO $$
DECLARE
    v_auth_user_id UUID;
    v_admin_cuid TEXT;
    v_admin_email TEXT := 'admin@nexa.test';
BEGIN
    -- auth.users에서 실제 UUID 가져오기
    SELECT id INTO v_auth_user_id
    FROM auth.users
    WHERE email = v_admin_email;
    
    IF v_auth_user_id IS NULL THEN
        RAISE EXCEPTION '❌ Auth user not found! Please create user in Authentication > Users first';
    END IF;
    
    RAISE NOTICE '📋 Found auth user: %', v_auth_user_id;
    
    -- app_users 확인 및 생성/업데이트
    SELECT cuid INTO v_admin_cuid
    FROM app_users
    WHERE email = v_admin_email;
    
    IF v_admin_cuid IS NULL THEN
        v_admin_cuid := 'admin_' || replace(gen_random_uuid()::text, '-', '');
        INSERT INTO app_users (cuid, auth_user_id, email, status, created_at, updated_at)
        VALUES (v_admin_cuid, v_auth_user_id, v_admin_email, 'active', NOW(), NOW());
        RAISE NOTICE '✅ Created app_users';
    ELSE
        UPDATE app_users SET auth_user_id = v_auth_user_id, updated_at = NOW() WHERE cuid = v_admin_cuid;
        RAISE NOTICE '✅ Updated app_users';
    END IF;
    
    -- user_infos
    INSERT INTO user_infos (user_cuid, username, full_name, role, status, registered_at, created_at, updated_at)
    VALUES (v_admin_cuid, 'admin', 'System Administrator', 'Admin', 'Active', NOW(), NOW(), NOW())
    ON CONFLICT (user_cuid) DO UPDATE
    SET username = EXCLUDED.username, full_name = EXCLUDED.full_name, role = EXCLUDED.role, updated_at = NOW();
    RAISE NOTICE '✅ Created/Updated user_infos';
    
    -- profiles
    INSERT INTO profiles (id, user_cuid, username, full_name, avatar_url, updated_at)
    VALUES (v_auth_user_id, v_admin_cuid, 'admin', 'System Administrator', NULL, NOW())
    ON CONFLICT (id) DO UPDATE
    SET user_cuid = EXCLUDED.user_cuid, username = EXCLUDED.username, updated_at = NOW();
    RAISE NOTICE '✅ Created/Updated profiles';
    
    -- organization_users
    IF EXISTS (SELECT 1 FROM organization_users WHERE user_cuid = v_admin_cuid) THEN
        UPDATE organization_users
        SET user_id = v_auth_user_id, role = 'Admin', status = 'active'
        WHERE user_cuid = v_admin_cuid;
        RAISE NOTICE '✅ Updated organization_users';
    ELSE
        INSERT INTO organization_users (user_id, user_cuid, role, status, registered_at, approval_memo)
        VALUES (v_auth_user_id, v_admin_cuid, 'Admin', 'active', NOW(), 'System Admin');
        RAISE NOTICE '✅ Created organization_users';
    END IF;
    
    -- user_roles
    IF NOT EXISTS (SELECT 1 FROM user_roles WHERE user_cuid = v_admin_cuid AND role_code = 'Admin') THEN
        INSERT INTO user_roles (user_cuid, role_code, assigned_at)
        VALUES (v_admin_cuid, 'Admin', NOW());
        RAISE NOTICE '✅ Added Admin role to user_roles';
    END IF;
    
    RAISE NOTICE '🎉 Admin account successfully created!';
    RAISE NOTICE '📧 Email: admin@nexa.test';
    RAISE NOTICE '🔑 Password: admin123';
END $$;

-- Step 3: 최종 확인
SELECT 
    '✅ VERIFICATION' AS status,
    au.email,
    au.auth_user_id,
    ui.username,
    ui.full_name,
    ou.role AS org_role,
    ARRAY_AGG(ur.role_code) AS user_roles
FROM app_users au
JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN organization_users ou ON ou.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
WHERE au.email = 'admin@nexa.test'
GROUP BY au.email, au.auth_user_id, ui.username, ui.full_name, ou.role;
```

4. **Run** 버튼 클릭 (또는 `Cmd/Ctrl + Enter`)

---

## ✅ 3단계: 결과 확인

성공하면 다음 메시지들이 표시됩니다:

```
✅ Created/Updated app_users
✅ Created/Updated user_infos
✅ Created/Updated profiles
✅ Created/Updated organization_users
✅ Added Admin role to user_roles
🎉 Admin account successfully created!
```

마지막 SELECT 결과:
| status | email | auth_user_id | username | full_name | org_role | user_roles |
|--------|-------|--------------|----------|-----------|----------|------------|
| ✅ VERIFICATION | admin@nexa.test | (UUID) | admin | System Administrator | Admin | {Admin} |

---

## 🔐 4단계: 로그인 테스트

```bash
# 서버 실행
cd src/NexaCRM.WebServer
dotnet run
```

브라우저에서:
```
http://localhost:5000/login

Username: admin
Password: admin123
```

✅ **로그인 성공!** 🎉

---

## 🔧 문제 해결

### "Auth user not found" 오류
- 1단계를 먼저 완료했는지 확인
- Authentication > Users에서 admin@nexa.test가 있는지 확인

### 계정 삭제하고 다시 만들기
```sql
-- 모든 관련 레코드 삭제
DELETE FROM user_roles WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM organization_users WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM profiles WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM user_infos WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
DELETE FROM app_users WHERE email = 'admin@nexa.test';

-- Authentication > Users에서도 삭제 필요
```

---

## 📊 추가 확인 쿼리

### 전체 계정 정보 확인
```sql
SELECT * FROM user_account_overview WHERE email = 'admin@nexa.test';
```

### 각 테이블별 확인
```sql
SELECT 'app_users' AS table_name, email, auth_user_id, status FROM app_users WHERE email = 'admin@nexa.test'
UNION ALL
SELECT 'user_infos', username, NULL, status FROM user_infos WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test')
UNION ALL
SELECT 'profiles', username, id::text, NULL FROM profiles WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test')
UNION ALL
SELECT 'organization_users', role, user_id::text, status FROM organization_users WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test')
UNION ALL
SELECT 'user_roles', role_code, NULL, NULL FROM user_roles WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');
```

---

**이제 완벽한 Admin 계정이 준비되었습니다!** 🚀

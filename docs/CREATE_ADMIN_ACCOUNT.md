# Admin 계정 생성 가이드

## ⚡ 빠른 시작 (Supabase Dashboard - 가장 확실함)

**가장 확실하고 빠른 방법입니다:**

### Step 1: Supabase Dashboard에서 SQL 실행

1. Supabase Dashboard 로그인: https://supabase.com/dashboard
2. 프로젝트 선택
3. 왼쪽 메뉴 > **SQL Editor** 클릭
4. **New Query** 버튼 클릭
5. 아래 SQL을 복사해서 붙여넣기:

```sql
-- Make auth_user_id and user_id nullable (if not already)
ALTER TABLE app_users ALTER COLUMN auth_user_id DROP NOT NULL;
ALTER TABLE organization_users ALTER COLUMN user_id DROP NOT NULL;

-- Create admin account structure
DO $$
DECLARE
    v_admin_cuid TEXT := 'admin_' || replace(gen_random_uuid()::text, '-', '');
    v_admin_email TEXT := 'admin@nexa.test';
BEGIN
    -- Skip if already exists
    IF EXISTS (SELECT 1 FROM app_users WHERE email = v_admin_email) THEN
        RAISE NOTICE 'Admin account already exists';
        RETURN;
    END IF;

    -- Insert app_users
    INSERT INTO app_users (cuid, email, status, created_at, updated_at)
    VALUES (v_admin_cuid, v_admin_email, 'active', NOW(), NOW());

    -- Insert user_infos
    INSERT INTO user_infos (user_cuid, username, full_name, role, status, registered_at, created_at, updated_at)
    VALUES (v_admin_cuid, 'admin', 'System Administrator', 'Admin', 'Active', NOW(), NOW(), NOW());

    -- Insert organization_users (user_id will be NULL until Supabase Auth registration)
    INSERT INTO organization_users (user_id, user_cuid, role, status, registered_at, approval_memo)
    VALUES (NULL, v_admin_cuid, 'Admin', 'active', NOW(), 'System Admin - Auto Created');

    RAISE NOTICE 'Admin account created: %', v_admin_email;
END $$;

-- Verify
SELECT email, status, cuid FROM app_users WHERE email = 'admin@nexa.test';
```

6. **Run** 버튼 클릭
7. 결과 확인 (admin@nexa.test가 표시되어야 함)

### Step 2: 회원가입으로 Auth 연결

이제 애플리케이션에서 회원가입:

1. 서버 실행:
   ```bash
   cd src/NexaCRM.WebServer
   dotnet run
   ```

2. 회원가입 페이지: `http://localhost:5000/register`

3. 다음 정보로 가입:
   - **Email**: `admin@nexa.test`
   - **Password**: `admin123`
   - **Username**: `admin`
   - **Full Name**: `System Administrator`

4. 회원가입하면 자동으로 Supabase Auth와 연결됩니다.

---

## 방법 2: 회원가입 페이지만 사용

위의 SQL을 실행하지 않고, 바로 회원가입하는 방법:

1. 서버 실행:
   ```bash
   cd src/NexaCRM.WebServer
   dotnet run
   ```

2. 브라우저에서 회원가입: `http://localhost:5000/register`

3. 다음 정보로 회원가입:
   - **Email**: `admin@nexa.test`
   - **Password**: `admin123`
   - **Username**: `admin`
   - **Full Name**: `System Administrator`

4. 회원가입 후, Supabase Dashboard에서 계정 상태를 'active'로 변경:
   - Dashboard > Table Editor > `app_users` 테이블
   - `admin@nexa.test` 찾기
   - `status` 컬럼을 `'active'`로 변경
   - Save

## 방법 2: Node.js 스크립트 사용

서버가 실행 중일 때 자동으로 생성합니다:

1. 서버 실행:
   ```bash
   cd src/NexaCRM.WebServer
   dotnet run
   ```

2. 새 터미널에서 스크립트 실행:
   ```bash
   node scripts/create-admin-account.js
   ```

3. 다른 서버 URL을 사용하는 경우:
   ```bash
   SERVER_URL=http://localhost:5000 node scripts/create-admin-account.js
   ```

## 방법 3: Supabase Dashboard 직접 사용

1. Supabase Dashboard 로그인:
   ```
   https://supabase.com/dashboard/project/YOUR_PROJECT_ID
   ```

2. Authentication > Users > Add User:
   - Email: `admin@nexa.test`
   - Password: `admin123`
   - Auto Confirm User: ✅ (체크)

3. 위 방법으로 auth 계정을 생성한 후, 회원가입 페이지에서 같은 이메일로 가입하면 연결됩니다.

## 로그인

생성된 계정으로 로그인:

1. 로그인 페이지로 이동:
   ```
   http://localhost:5000/login
   ```

2. 로그인 정보:
   - **Username**: `admin` 또는 `admin@nexa.test`
   - **Password**: `admin123`

## 문제 해결

### "관리자 승인 대기 중" 오류
계정 상태가 'Pending'일 경우 발생합니다.

**해결 방법**:
1. Supabase Dashboard > Table Editor > `app_users`
2. `admin@nexa.test` 찾기
3. `status` 필드를 `'active'`로 변경
4. 저장 후 다시 로그인

### "Database error querying schema" 오류
Auth 계정이 없는 경우 발생합니다.

**해결 방법**:
1. 방법 1 또는 방법 2로 다시 회원가입
2. 또는 Supabase Dashboard에서 직접 Auth 사용자 생성

## 기존 데모 계정

다음 계정들도 사용 가능합니다:

- **Manager**: `manager@nexa.test` / 비밀번호 확인 필요
- **Sales**: `sales@nexa.test` / 비밀번호 확인 필요  
- **Developer**: `develop@nexa.test` / 비밀번호 확인 필요

이 계정들은 이미 status='active'로 설정되어 있습니다.

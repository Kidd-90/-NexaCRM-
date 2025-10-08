-- ============================================
-- Complete Admin Account Creation & Auth Linking
-- Supabase Dashboard SQL Editor용
-- ============================================

-- Step 1: 테이블 제약조건 수정 (nullable로 변경)
ALTER TABLE app_users 
ALTER COLUMN auth_user_id DROP NOT NULL;

ALTER TABLE organization_users 
ALTER COLUMN user_id DROP NOT NULL;

ALTER TABLE user_roles 
ALTER COLUMN user_id DROP NOT NULL;

-- Step 1.5: role_definitions 테이블에 기본 역할 추가
INSERT INTO role_definitions (role_code, role_name, description, created_at)
VALUES 
    ('Admin', 'Administrator', 'System administrator with full access', NOW()),
    ('Manager', 'Manager', 'Manager with elevated permissions', NOW()),
    ('Sales', 'Sales Representative', 'Sales team member', NOW()),
    ('User', 'User', 'Standard user with basic permissions', NOW())
ON CONFLICT (role_code) DO UPDATE
SET role_name = EXCLUDED.role_name,
    description = EXCLUDED.description,
    updated_at = NOW();

RAISE NOTICE '✅ Added/Updated role definitions: Admin, Manager, Sales, User';

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
        RAISE EXCEPTION '❌ Auth user not found! Please create user in Authentication > Users first with email: %', v_admin_email;
    END IF;
    
    RAISE NOTICE '📋 Found auth user: %', v_auth_user_id;
    
    -- app_users 확인 및 생성/업데이트
    SELECT cuid INTO v_admin_cuid
    FROM app_users
    WHERE email = v_admin_email;
    
    IF v_admin_cuid IS NULL THEN
        -- app_users 레코드가 없으면 생성
        v_admin_cuid := 'admin_' || replace(gen_random_uuid()::text, '-', '');
        
        INSERT INTO app_users (cuid, auth_user_id, email, status, created_at, updated_at)
        VALUES (v_admin_cuid, v_auth_user_id, v_admin_email, 'active', NOW(), NOW());
        
        RAISE NOTICE '✅ Created app_users record with CUID: %', v_admin_cuid;
    ELSE
        -- 이미 있으면 auth_user_id만 업데이트
        UPDATE app_users 
        SET auth_user_id = v_auth_user_id,
            updated_at = NOW()
        WHERE cuid = v_admin_cuid;
        
        RAISE NOTICE '✅ Updated app_users.auth_user_id for CUID: %', v_admin_cuid;
    END IF;
    
    -- user_infos 생성/업데이트
    INSERT INTO user_infos (user_cuid, username, full_name, role, status, registered_at, created_at, updated_at)
    VALUES (v_admin_cuid, 'admin', 'System Administrator', 'Admin', 'Active', NOW(), NOW(), NOW())
    ON CONFLICT (user_cuid) DO UPDATE
    SET username = EXCLUDED.username,
        full_name = EXCLUDED.full_name,
        role = EXCLUDED.role,
        status = EXCLUDED.status,
        updated_at = NOW();
    
    RAISE NOTICE '✅ Created/Updated user_infos record';
    
    -- profiles 테이블에 추가
    INSERT INTO profiles (id, user_cuid, username, full_name, avatar_url, updated_at)
    VALUES (v_auth_user_id, v_admin_cuid, 'admin', 'System Administrator', NULL, NOW())
    ON CONFLICT (id) DO UPDATE
    SET user_cuid = EXCLUDED.user_cuid,
        username = EXCLUDED.username,
        full_name = EXCLUDED.full_name,
        updated_at = NOW();
    
    RAISE NOTICE '✅ Created/Updated profiles record';
    
    -- organization_users 생성/업데이트
    -- user_cuid에 UNIQUE 제약조건이 없으므로 먼저 확인 후 INSERT/UPDATE
    IF EXISTS (SELECT 1 FROM organization_users WHERE user_cuid = v_admin_cuid) THEN
        UPDATE organization_users
        SET user_id = v_auth_user_id,
            role = 'Admin',
            status = 'active',
            approval_memo = 'System Administrator - Auto Created'
        WHERE user_cuid = v_admin_cuid;
        
        RAISE NOTICE '✅ Updated organization_users record';
    ELSE
        INSERT INTO organization_users (user_id, user_cuid, role, status, registered_at, approval_memo)
        VALUES (v_auth_user_id, v_admin_cuid, 'Admin', 'active', NOW(), 'System Administrator - Auto Created');
        
        RAISE NOTICE '✅ Created organization_users record';
    END IF;
    
    -- user_roles에 Admin 역할 추가
    IF NOT EXISTS (SELECT 1 FROM user_roles WHERE user_cuid = v_admin_cuid AND role_code = 'Admin') THEN
        INSERT INTO user_roles (user_id, user_cuid, role_code, assigned_at)
        VALUES (v_auth_user_id, v_admin_cuid, 'Admin', NOW());
        
        RAISE NOTICE '✅ Added Admin role to user_roles table';
    ELSE
        -- 이미 있으면 user_id 업데이트
        UPDATE user_roles
        SET user_id = v_auth_user_id
        WHERE user_cuid = v_admin_cuid AND role_code = 'Admin';
        
        RAISE NOTICE '✅ Updated Admin role in user_roles table';
    END IF;
    
    RAISE NOTICE '';
    RAISE NOTICE '🎉 Admin account successfully created and linked!';
    RAISE NOTICE '';
    RAISE NOTICE '📝 Login credentials:';
    RAISE NOTICE '   Email/Username: admin@nexa.test or admin';
    RAISE NOTICE '   Password: admin123';
    RAISE NOTICE '';
    RAISE NOTICE '🔑 Auth User ID: %', v_auth_user_id;
    RAISE NOTICE '🆔 CUID: %', v_admin_cuid;
END $$;

-- Step 3: 최종 확인
SELECT 
    '✅ FINAL VERIFICATION' AS status,
    au.email,
    au.auth_user_id,
    au.status AS app_status,
    ui.username,
    ui.full_name,
    ui.role AS user_role,
    p.id AS profile_id,
    ou.role AS org_role,
    COALESCE(
        ARRAY_AGG(ur.role_code) FILTER (WHERE ur.role_code IS NOT NULL),
        ARRAY[]::TEXT[]
    ) AS assigned_roles
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN profiles p ON p.id = au.auth_user_id
LEFT JOIN organization_users ou ON ou.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
WHERE au.email = 'admin@nexa.test'
GROUP BY au.email, au.auth_user_id, au.status, ui.username, ui.full_name, ui.role, p.id, ou.role;

-- 추가: user_account_overview 뷰로도 확인
SELECT 
    '✅ VIEW VERIFICATION' AS status,
    cuid,
    auth_user_id,
    email,
    username,
    full_name,
    status,
    role_codes
FROM user_account_overview
WHERE email = 'admin@nexa.test';

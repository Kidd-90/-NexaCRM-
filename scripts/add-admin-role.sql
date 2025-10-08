-- ============================================
-- Admin Role 빠른 추가 스크립트
-- 이미 Admin 계정이 있는 경우 Role만 추가
-- ============================================

DO $$
DECLARE
    v_auth_user_id UUID;
    v_admin_cuid TEXT;
    v_admin_email TEXT := 'admin@nexa.test';
BEGIN
    -- 1. Admin 계정 정보 가져오기
    SELECT cuid, auth_user_id INTO v_admin_cuid, v_auth_user_id
    FROM app_users
    WHERE email = v_admin_email;
    
    IF v_admin_cuid IS NULL THEN
        RAISE EXCEPTION '❌ Admin account not found! Please run create-admin-complete.sql first.';
    END IF;
    
    RAISE NOTICE '📋 Found admin account: CUID=%, Auth ID=%', v_admin_cuid, v_auth_user_id;
    
    -- 2. role_definitions에 Admin 역할이 있는지 확인
    IF NOT EXISTS (SELECT 1 FROM role_definitions WHERE role_code = 'Admin') THEN
        INSERT INTO role_definitions (role_code, role_name, description, created_at)
        VALUES ('Admin', 'Administrator', 'System administrator with full access', NOW());
        RAISE NOTICE '✅ Created Admin role definition';
    END IF;
    
    -- 3. user_roles에 Admin 역할 추가
    IF EXISTS (SELECT 1 FROM user_roles WHERE user_cuid = v_admin_cuid AND role_code = 'Admin') THEN
        -- 이미 있으면 user_id 업데이트
        UPDATE user_roles
        SET user_id = v_auth_user_id,
            assigned_at = COALESCE(assigned_at, NOW())
        WHERE user_cuid = v_admin_cuid AND role_code = 'Admin';
        
        RAISE NOTICE '✅ Updated existing Admin role';
    ELSE
        -- 없으면 새로 추가
        INSERT INTO user_roles (user_id, user_cuid, role_code, assigned_at)
        VALUES (v_auth_user_id, v_admin_cuid, 'Admin', NOW());
        
        RAISE NOTICE '✅ Added new Admin role';
    END IF;
    
    RAISE NOTICE '';
    RAISE NOTICE '🎉 Admin role added successfully!';
    RAISE NOTICE '';
END $$;

-- 검증 쿼리
SELECT 
    '=== Verification ===' AS section,
    email,
    username,
    role_codes,
    array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';

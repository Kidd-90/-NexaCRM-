-- ============================================
-- 🚨 긴급 수정: Admin Role 추가
-- 로그인 루프 해결
-- ============================================

-- 현재 상태 확인
SELECT 'BEFORE' AS stage, email, username, role_codes, array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';

-- Admin Role 추가
DO $$
DECLARE
    v_auth_user_id UUID;
    v_admin_cuid TEXT;
    v_admin_email TEXT := 'admin@nexa.test';
BEGIN
    -- Admin 계정 정보 가져오기
    SELECT cuid, auth_user_id INTO v_admin_cuid, v_auth_user_id
    FROM app_users
    WHERE email = v_admin_email;
    
    IF v_admin_cuid IS NULL THEN
        RAISE EXCEPTION '❌ Admin account not found! Run create-admin-complete.sql first.';
    END IF;
    
    RAISE NOTICE '📋 Admin CUID: %, Auth ID: %', v_admin_cuid, v_auth_user_id;
    
    -- role_definitions에 Admin 추가 (없으면)
    INSERT INTO role_definitions (code, name, description, created_at)
    VALUES ('Admin', 'Administrator', 'System administrator with full access', NOW())
    ON CONFLICT (code) DO NOTHING;
    
    RAISE NOTICE '✅ Role definition checked/added';
    
    -- user_roles에 Admin 추가
    IF EXISTS (SELECT 1 FROM user_roles WHERE user_cuid = v_admin_cuid AND role_code = 'Admin') THEN
        -- 이미 있으면 user_id 업데이트
        UPDATE user_roles
        SET user_id = v_auth_user_id
        WHERE user_cuid = v_admin_cuid AND role_code = 'Admin';
        
        RAISE NOTICE '✅ Updated existing Admin role';
    ELSE
        -- 없으면 새로 추가
        INSERT INTO user_roles (user_id, user_cuid, role_code, assigned_at)
        VALUES (v_auth_user_id, v_admin_cuid, 'Admin', NOW());
        
        RAISE NOTICE '🎉 Added new Admin role!';
    END IF;
END $$;

-- 수정 후 상태 확인
SELECT 'AFTER' AS stage, email, username, role_codes, array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';

-- 상세 확인
SELECT 
    'DETAILED CHECK' AS info,
    au.email,
    au.cuid,
    ui.username,
    ur.role_code,
    rd.name AS role_name
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
LEFT JOIN role_definitions rd ON rd.code = ur.role_code
WHERE au.email = 'admin@nexa.test';

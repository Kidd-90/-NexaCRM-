-- ============================================
-- 🚨 완전 통합 버전 (컬럼명 수정)
-- role_definitions.code, user_roles.role_code
-- ============================================

-- Step 1: role_definitions 추가 (컬럼명: code, name, description)
INSERT INTO role_definitions (code, name, description, created_at)
VALUES 
    ('Admin', 'Administrator', 'System administrator with full access', NOW()),
    ('Manager', 'Manager', 'Team manager with management capabilities', NOW()),
    ('Sales', 'Sales Representative', 'Sales team member', NOW()),
    ('User', 'Standard User', 'Basic user with limited access', NOW())
ON CONFLICT (code) DO NOTHING;

-- 추가 확인
SELECT '✅ ROLE DEFINITIONS' AS step, code, name, description
FROM role_definitions
ORDER BY code;

-- Step 2: Admin 계정에 Admin 역할 할당
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
        RAISE EXCEPTION '❌ Admin account not found! Email: %', v_admin_email;
    END IF;
    
    RAISE NOTICE '📋 Admin CUID: %, Auth ID: %', v_admin_cuid, v_auth_user_id;
    
    -- user_roles에 Admin 역할 추가 (컬럼명: role_code)
    INSERT INTO user_roles (user_id, user_cuid, role_code, assigned_at)
    VALUES (v_auth_user_id, v_admin_cuid, 'Admin', NOW())
    ON CONFLICT (user_cuid, role_code) DO UPDATE
    SET user_id = EXCLUDED.user_id,
        assigned_at = NOW();
    
    RAISE NOTICE '🎉 Admin role assigned successfully!';
END $$;

-- Step 3: 최종 확인
SELECT 
    '✅ FINAL CHECK' AS step,
    email,
    username,
    role_codes,
    array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';

-- 상세 확인
SELECT 
    '📋 DETAILED' AS step,
    au.email,
    au.cuid,
    ui.username,
    ur.role_code,
    rd.name AS role_name,
    rd.description
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
LEFT JOIN role_definitions rd ON rd.code = ur.role_code
WHERE au.email = 'admin@nexa.test';

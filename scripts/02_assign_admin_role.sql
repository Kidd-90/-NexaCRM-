-- ============================================
-- Step 2: Admin 계정에 Admin 역할 할당
-- ============================================

-- 현재 admin 계정 상태 확인
SELECT 
    'ADMIN ACCOUNT' AS info,
    au.email,
    au.cuid,
    au.auth_user_id,
    ui.username,
    COALESCE(array_agg(ur.role_code) FILTER (WHERE ur.role_code IS NOT NULL), '{}') AS current_roles
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
WHERE au.email = 'admin@nexa.test'
GROUP BY au.email, au.cuid, au.auth_user_id, ui.username;

-- Admin 역할 할당
DO $$
DECLARE
    v_auth_user_id UUID;
    v_admin_cuid TEXT;
BEGIN
    -- Admin 계정 정보 가져오기
    SELECT cuid, auth_user_id INTO v_admin_cuid, v_auth_user_id
    FROM app_users
    WHERE email = 'admin@nexa.test';
    
    IF v_admin_cuid IS NULL THEN
        RAISE EXCEPTION '❌ Admin account (admin@nexa.test) not found in app_users!';
    END IF;
    
    RAISE NOTICE '📋 Found admin account - CUID: %, Auth ID: %', v_admin_cuid, v_auth_user_id;
    
    -- user_roles에 Admin 역할 추가
    INSERT INTO user_roles (user_id, user_cuid, role_code, assigned_at)
    VALUES (v_auth_user_id, v_admin_cuid, 'Admin', NOW())
    ON CONFLICT (user_cuid, role_code) DO UPDATE
    SET user_id = EXCLUDED.user_id,
        assigned_at = NOW();
    
    RAISE NOTICE '✅ Admin role assigned successfully!';
END $$;

-- 할당 후 확인
SELECT 
    'AFTER ASSIGNMENT' AS info,
    au.email,
    au.cuid,
    ui.username,
    array_agg(ur.role_code ORDER BY ur.role_code) AS assigned_roles,
    count(ur.role_code) AS role_count
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
WHERE au.email = 'admin@nexa.test'
GROUP BY au.email, au.cuid, ui.username;

-- user_account_overview에서 최종 확인
SELECT 
    'VIEW CHECK' AS info,
    email,
    username,
    role_codes,
    array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';

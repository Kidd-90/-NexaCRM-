-- ============================================
-- Admin 계정 Role 확인 스크립트
-- ============================================

-- 1. Admin 계정 기본 정보
SELECT '=== Admin Account Info ===' AS section;
SELECT 
    cuid,
    email,
    auth_user_id,
    status
FROM app_users
WHERE email = 'admin@nexa.test';

-- 2. user_infos 확인
SELECT '=== User Infos ===' AS section;
SELECT 
    user_cuid,
    username,
    full_name,
    role,
    status
FROM user_infos
WHERE username = 'admin';

-- 3. user_roles 확인 (중요!)
SELECT '=== User Roles ===' AS section;
SELECT 
    ur.user_id,
    ur.user_cuid,
    ur.role_code,
    ur.assigned_at,
    rd.role_name
FROM user_roles ur
LEFT JOIN role_definitions rd ON rd.role_code = ur.role_code
WHERE ur.user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');

-- 4. role_definitions 확인
SELECT '=== Role Definitions ===' AS section;
SELECT 
    role_code,
    role_name,
    description
FROM role_definitions
ORDER BY role_code;

-- 5. organization_users 확인
SELECT '=== Organization Users ===' AS section;
SELECT 
    user_id,
    user_cuid,
    role,
    status
FROM organization_users
WHERE user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');

-- 6. user_account_overview 뷰 확인 (가장 중요!)
SELECT '=== User Account Overview (Final Check) ===' AS section;
SELECT 
    cuid,
    email,
    username,
    full_name,
    status,
    role_codes,  -- 이게 비어있으면 문제!
    array_length(role_codes, 1) AS role_count
FROM user_account_overview
WHERE email = 'admin@nexa.test';

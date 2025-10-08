-- Check existing accounts in app_users
SELECT 
    email, 
    status, 
    auth_user_id,
    cuid,
    created_at
FROM app_users 
ORDER BY created_at DESC 
LIMIT 10;

-- Check if admin account exists
SELECT 
    au.email,
    au.status AS app_status,
    au.auth_user_id,
    ui.username,
    ui.full_name,
    ui.role,
    ui.status AS user_status
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
WHERE au.email LIKE '%admin%'
   OR ui.username LIKE '%admin%';

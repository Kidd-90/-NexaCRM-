-- Activate demo accounts (manager, sales, develop) for login
-- These accounts need status='active' to pass the IsAccountActive check

UPDATE app_users 
SET status = 'active', updated_at = NOW()
WHERE email IN ('manager@nexa.test', 'sales@nexa.test', 'develop@nexa.test');

-- Also update user_infos status if the table has a status column
UPDATE user_infos
SET status = 'Active', updated_at = NOW()
WHERE user_cuid IN (
    SELECT cuid FROM app_users 
    WHERE email IN ('manager@nexa.test', 'sales@nexa.test', 'develop@nexa.test')
);

-- Verify the changes
SELECT 
    au.email, 
    au.status AS app_user_status, 
    ui.status AS user_info_status,
    ui.username
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
WHERE au.email IN ('manager@nexa.test', 'sales@nexa.test', 'develop@nexa.test');

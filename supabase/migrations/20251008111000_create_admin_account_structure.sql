-- Create admin account directly in the database
-- This script creates a complete admin account structure

-- Step 1: Create a function to generate admin account
CREATE OR REPLACE FUNCTION create_admin_account()
RETURNS TABLE (
    result_message TEXT,
    result_email TEXT,
    result_cuid TEXT,
    result_status TEXT
) AS $$
DECLARE
    v_admin_cuid TEXT;
    v_admin_email TEXT := 'admin@nexa.test';
    v_existing_count INTEGER;
BEGIN
    -- Check if admin already exists
    SELECT COUNT(*) INTO v_existing_count
    FROM app_users au
    WHERE au.email = v_admin_email;

    IF v_existing_count > 0 THEN
        RETURN QUERY
        SELECT 
            'Admin account already exists'::TEXT AS result_message,
            au.email AS result_email,
            au.cuid AS result_cuid,
            au.status::TEXT AS result_status
        FROM app_users au
        WHERE au.email = v_admin_email;
        RETURN;
    END IF;

    -- Generate CUID
    v_admin_cuid := 'admin_' || replace(gen_random_uuid()::text, '-', '');

    -- Insert into app_users (auth_user_id will be NULL until user registers through Supabase Auth)
    INSERT INTO app_users (cuid, email, status, created_at, updated_at)
    VALUES (
        v_admin_cuid,
        v_admin_email,
        'active',  -- Set as active immediately
        NOW(),
        NOW()
    );

    -- Insert into user_infos
    INSERT INTO user_infos (
        user_cuid, 
        username, 
        full_name, 
        role, 
        status, 
        registered_at, 
        created_at, 
        updated_at
    )
    VALUES (
        v_admin_cuid,
        'admin',
        'System Administrator',
        'Admin',
        'Active',
        NOW(),
        NOW(),
        NOW()
    );

    -- Note: user_profiles table will be populated when user registers through Supabase Auth
    -- because user_profiles.id has a foreign key constraint to auth.users(id)

    -- Insert into organization_users
    INSERT INTO organization_users (
        user_cuid, 
        role, 
        status, 
        registered_at, 
        approval_memo
    )
    VALUES (
        v_admin_cuid,
        'Admin',
        'active',
        NOW(),
        'System Administrator Account - Auto Created'
    );

    RETURN QUERY
    SELECT 
        'Admin account created successfully. Use email: admin@nexa.test to register with password: admin123'::TEXT AS result_message,
        v_admin_email AS result_email,
        v_admin_cuid AS result_cuid,
        'active'::TEXT AS result_status;
END;
$$ LANGUAGE plpgsql;

-- Execute the function to create admin account
SELECT * FROM create_admin_account();

-- Drop the function after use
DROP FUNCTION IF EXISTS create_admin_account();

-- Verification: Check what was created
SELECT 
    'app_users' AS table_name,
    au.email,
    au.status,
    au.cuid,
    au.auth_user_id
FROM app_users au
WHERE au.email = 'admin@nexa.test'

UNION ALL

SELECT 
    'user_infos' AS table_name,
    ui.username || ' (' || ui.full_name || ')' AS email_info,
    ui.status,
    ui.user_cuid AS cuid,
    NULL AS auth_user_id
FROM user_infos ui
WHERE ui.user_cuid IN (SELECT cuid FROM app_users WHERE email = 'admin@nexa.test');

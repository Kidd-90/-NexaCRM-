-- Make auth_user_id and user_id nullable
-- This allows creating app-level accounts before Supabase Auth registration

-- Make app_users.auth_user_id nullable
ALTER TABLE app_users 
ALTER COLUMN auth_user_id DROP NOT NULL;

-- Make organization_users.user_id nullable
ALTER TABLE organization_users 
ALTER COLUMN user_id DROP NOT NULL;

-- Add comments explaining the nullable fields
COMMENT ON COLUMN app_users.auth_user_id IS 
'Supabase Auth user ID. Can be NULL before user completes registration through Supabase Auth. 
Will be populated when user registers or signs in.';

COMMENT ON COLUMN organization_users.user_id IS 
'Supabase Auth user ID. Can be NULL before user completes registration through Supabase Auth. 
Will be populated when user registers or signs in.';

-- Verify the changes
SELECT 
    table_name,
    column_name,
    is_nullable,
    data_type
FROM information_schema.columns
WHERE (table_name = 'app_users' AND column_name = 'auth_user_id')
   OR (table_name = 'organization_users' AND column_name = 'user_id')
ORDER BY table_name, column_name;

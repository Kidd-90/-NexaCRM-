-- Recreate user_account_overview view to fix schema query errors
-- This view consolidates user information from multiple tables

DROP VIEW IF EXISTS user_account_overview;

CREATE OR REPLACE VIEW user_account_overview AS
SELECT
  au.cuid,
  au.auth_user_id,
  au.email,
  au.status,
  au.created_at AS account_created_at,
  au.updated_at AS account_updated_at,
  ui.username,
  ui.full_name,
  ui.password_hash,
  ui.department,
  ui.job_title,
  ui.phone_number,
  ui.created_at AS profile_created_at,
  ui.updated_at AS profile_updated_at,
  COALESCE(
    ARRAY_AGG(ur.role_code ORDER BY ur.role_code)
      FILTER (WHERE ur.role_code IS NOT NULL),
    ARRAY[]::TEXT[]
  ) AS role_codes
FROM app_users au
LEFT JOIN user_infos ui ON ui.user_cuid = au.cuid
LEFT JOIN user_roles ur ON ur.user_cuid = au.cuid
GROUP BY
  au.cuid,
  au.auth_user_id,
  au.email,
  au.status,
  au.created_at,
  au.updated_at,
  ui.username,
  ui.full_name,
  ui.password_hash,
  ui.department,
  ui.job_title,
  ui.phone_number,
  ui.created_at,
  ui.updated_at;

COMMENT ON VIEW user_account_overview IS 'Consolidated view of user account information from app_users, user_infos, and user_roles';

-- Grant access to the view
GRANT SELECT ON user_account_overview TO authenticated;
GRANT SELECT ON user_account_overview TO anon;

-- Add password_hash column to user_infos table if it doesn't exist
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'user_infos' AND column_name = 'password_hash') THEN
    ALTER TABLE user_infos ADD COLUMN password_hash TEXT;
  END IF;
END $$;

-- Drop and recreate the user_account_overview view to include password_hash from user_infos
DROP VIEW IF EXISTS user_account_overview;
CREATE VIEW user_account_overview AS
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
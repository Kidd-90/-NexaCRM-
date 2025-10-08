-- Add "User" role to role_definitions table
-- This is the default role for newly registered users

INSERT INTO role_definitions (code, name, description)
VALUES ('User', 'User', 'Basic user role with standard access permissions.')
ON CONFLICT (code) DO UPDATE
SET
  name = EXCLUDED.name,
  description = EXCLUDED.description,
  updated_at = NOW();

-- Verify the role was added
SELECT code, name, description FROM role_definitions WHERE code = 'User';

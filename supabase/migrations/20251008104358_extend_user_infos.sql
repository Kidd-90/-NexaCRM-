-- Extend user_infos table with registration and approval tracking fields

-- Add new columns to user_infos table
ALTER TABLE user_infos 
ADD COLUMN IF NOT EXISTS role TEXT DEFAULT 'Member',
ADD COLUMN IF NOT EXISTS status TEXT DEFAULT 'Pending',
ADD COLUMN IF NOT EXISTS registered_at TIMESTAMPTZ DEFAULT NOW(),
ADD COLUMN IF NOT EXISTS approved_at TIMESTAMPTZ,
ADD COLUMN IF NOT EXISTS approved_by TEXT,
ADD COLUMN IF NOT EXISTS approval_memo TEXT,
ADD COLUMN IF NOT EXISTS last_login_at TIMESTAMPTZ;

-- Create indexes for frequently queried fields
CREATE INDEX IF NOT EXISTS idx_user_infos_status ON user_infos(status);
CREATE INDEX IF NOT EXISTS idx_user_infos_role ON user_infos(role);

-- Add column comments for documentation
COMMENT ON COLUMN user_infos.role IS 'User role: Member, Admin, etc.';
COMMENT ON COLUMN user_infos.status IS 'User status: Pending, Active, Suspended, etc.';
COMMENT ON COLUMN user_infos.registered_at IS 'Timestamp when user registered';
COMMENT ON COLUMN user_infos.approved_at IS 'Timestamp when user was approved';
COMMENT ON COLUMN user_infos.approved_by IS 'User ID who approved this user';
COMMENT ON COLUMN user_infos.approval_memo IS 'Admin memo regarding approval';
COMMENT ON COLUMN user_infos.last_login_at IS 'Last login timestamp';

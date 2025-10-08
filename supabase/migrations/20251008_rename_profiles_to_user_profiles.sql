-- Rename profiles table to user_profiles
-- Migration Date: 2025-10-08

-- Step 1: Rename the table
ALTER TABLE IF EXISTS profiles RENAME TO user_profiles;

-- Step 2: Update comments
COMMENT ON TABLE user_profiles IS 'Public-facing user profile information.';
COMMENT ON COLUMN user_profiles.id IS 'Foreign key to auth.users.id.';
COMMENT ON COLUMN user_profiles.user_cuid IS 'Foreign key to user_infos.user_cuid.';

-- Step 3: Rename the trigger
DROP TRIGGER IF EXISTS on_profiles_updated ON user_profiles;
CREATE TRIGGER on_user_profiles_updated
  BEFORE UPDATE ON user_profiles
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

-- Step 4: Drop old RLS policies
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;

-- Step 5: Recreate RLS policies with the new table name
CREATE POLICY "Users can view their own profile"
  ON user_profiles FOR SELECT
  USING (auth.uid() = id);

CREATE POLICY "Users can update their own profile"
  ON user_profiles FOR UPDATE
  USING (auth.uid() = id)
  WITH CHECK (auth.uid() = id);

-- Verify the migration
SELECT 
    '✅ Table renamed successfully' AS status,
    schemaname, 
    tablename 
FROM pg_tables 
WHERE tablename = 'user_profiles';

SELECT 
    '✅ Trigger renamed successfully' AS status,
    tgname AS trigger_name,
    tgrelid::regclass AS table_name
FROM pg_trigger
WHERE tgname = 'on_user_profiles_updated';

SELECT 
    '✅ RLS policies recreated' AS status,
    policyname AS policy_name,
    tablename
FROM pg_policies
WHERE tablename = 'user_profiles';

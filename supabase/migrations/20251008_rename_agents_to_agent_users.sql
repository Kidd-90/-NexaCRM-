-- Rename agents table to agent_users
-- Migration Date: 2025-10-08

-- Step 1: Rename the table
ALTER TABLE IF EXISTS agents RENAME TO agent_users;

-- Step 2: Rename the index
DROP INDEX IF EXISTS idx_agents_user_cuid;
CREATE INDEX idx_agent_users_user_cuid ON agent_users(user_cuid);

-- Step 3: Rename the trigger
DROP TRIGGER IF EXISTS on_agents_updated ON agent_users;
CREATE TRIGGER on_agent_users_updated
  BEFORE UPDATE ON agent_users
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

-- Step 4: Drop old RLS policies
DROP POLICY IF EXISTS "Allow all access to read agents" ON agent_users;
DROP POLICY IF EXISTS "Allow all access to insert agents" ON agent_users;
DROP POLICY IF EXISTS "Allow all access to update agents" ON agent_users;
DROP POLICY IF EXISTS "Allow all access to delete agents" ON agent_users;

-- Step 5: Recreate RLS policies with the new table name
CREATE POLICY "Allow all access to read agent_users"
  ON agent_users
  FOR SELECT
  TO authenticated, anon, service_role
  USING (true);

CREATE POLICY "Allow all access to insert agent_users"
  ON agent_users
  FOR INSERT
  TO authenticated, anon, service_role
  WITH CHECK (true);

CREATE POLICY "Allow all access to update agent_users"
  ON agent_users
  FOR UPDATE
  TO authenticated, anon, service_role
  USING (true)
  WITH CHECK (true);

CREATE POLICY "Allow all access to delete agent_users"
  ON agent_users
  FOR DELETE
  TO authenticated, anon, service_role
  USING (true);

-- Verify the migration
SELECT 
    '✅ Table renamed successfully' AS status,
    schemaname, 
    tablename 
FROM pg_tables 
WHERE tablename = 'agent_users';

SELECT 
    '✅ Index renamed successfully' AS status,
    indexname
FROM pg_indexes
WHERE tablename = 'agent_users';

SELECT 
    '✅ Trigger renamed successfully' AS status,
    tgname AS trigger_name,
    tgrelid::regclass AS table_name
FROM pg_trigger
WHERE tgname = 'on_agent_users_updated';

SELECT 
    '✅ RLS policies recreated' AS status,
    policyname AS policy_name,
    tablename
FROM pg_policies
WHERE tablename = 'agent_users';

SELECT 
    '✅ Data preserved: ' || COUNT(*)::text || ' records' AS data_count
FROM agent_users;

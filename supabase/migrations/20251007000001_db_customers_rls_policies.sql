-- Row Level Security (RLS) policies for db_customers table

-- Enable RLS on db_customers table
ALTER TABLE db_customers ENABLE ROW LEVEL SECURITY;

-- Policy: Allow authenticated users to read all db_customers
CREATE POLICY "Allow authenticated users to read db_customers"
  ON db_customers
  FOR SELECT
  TO authenticated
  USING (true);

-- Policy: Allow authenticated users to insert db_customers
CREATE POLICY "Allow authenticated users to insert db_customers"
  ON db_customers
  FOR INSERT
  TO authenticated
  WITH CHECK (true);

-- Policy: Allow authenticated users to update db_customers
CREATE POLICY "Allow authenticated users to update db_customers"
  ON db_customers
  FOR UPDATE
  TO authenticated
  USING (true)
  WITH CHECK (true);

-- Policy: Allow authenticated users to delete db_customers
CREATE POLICY "Allow authenticated users to delete db_customers"
  ON db_customers
  FOR DELETE
  TO authenticated
  USING (true);

-- Note: These are basic policies. You may want to restrict access based on:
-- 1. assigned_to field matching the current user
-- 2. Organization/tenant-based access
-- 3. Role-based access control (RBAC)
-- 
-- Example of a more restrictive policy:
-- CREATE POLICY "Users can only see their assigned customers"
--   ON db_customers
--   FOR SELECT
--   TO authenticated
--   USING (assigned_to = auth.email() OR is_archived = false);

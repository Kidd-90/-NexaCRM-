-- Test queries to diagnose customer_notices table issues

-- 1. Check if table exists
SELECT EXISTS (
  SELECT FROM information_schema.tables 
  WHERE table_schema = 'public' 
  AND table_name = 'customer_notices'
) AS table_exists;

-- 2. Check table structure
SELECT 
  column_name,
  data_type,
  is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' 
AND table_name = 'customer_notices'
ORDER BY ordinal_position;

-- 3. Count records
SELECT COUNT(*) AS total_records FROM customer_notices;

-- 4. View all records
SELECT 
  id,
  title,
  category,
  importance,
  is_pinned,
  published_at,
  created_at
FROM customer_notices
ORDER BY is_pinned DESC, published_at DESC;

-- 5. Check RLS (Row Level Security) policies
SELECT 
  schemaname,
  tablename,
  policyname,
  permissive,
  roles,
  cmd,
  qual,
  with_check
FROM pg_policies
WHERE tablename = 'customer_notices';

-- 6. Check if RLS is enabled
SELECT 
  schemaname,
  tablename,
  rowsecurity
FROM pg_tables
WHERE tablename = 'customer_notices';

-- Quick fix script for customer_notices table access issues
-- Run this in Supabase Dashboard > SQL Editor

-- Step 1: Verify table exists
DO $$
BEGIN
  IF EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = 'customer_notices'
  ) THEN
    RAISE NOTICE 'Table customer_notices exists ✓';
  ELSE
    RAISE EXCEPTION 'Table customer_notices does NOT exist! Please run schema.sql first.';
  END IF;
END $$;

-- Step 2: Disable RLS for development (enable proper policies in production)
ALTER TABLE customer_notices DISABLE ROW LEVEL SECURITY;
RAISE NOTICE 'Row Level Security disabled for customer_notices ✓';

-- Step 3: Refresh schema cache
NOTIFY pgrst, 'reload schema';
RAISE NOTICE 'Schema cache reloaded ✓';

-- Step 4: Check if data exists
DO $$
DECLARE
  record_count INTEGER;
BEGIN
  SELECT COUNT(*) INTO record_count FROM customer_notices;
  RAISE NOTICE 'Found % records in customer_notices', record_count;
  
  IF record_count = 0 THEN
    RAISE NOTICE 'No data found. Please run insert_customer_notices_sample_data.sql';
  END IF;
END $$;

-- Step 5: Display current data
SELECT 
  id,
  title,
  category,
  importance,
  is_pinned,
  published_at
FROM customer_notices
ORDER BY is_pinned DESC, published_at DESC
LIMIT 10;

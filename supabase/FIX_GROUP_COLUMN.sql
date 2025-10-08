-- Fix: Rename 'group' column to 'customer_group' to avoid reserved keyword issues
-- Date: 2025-10-08
-- Issue: PostgreSQL 'group' is a reserved keyword causing schema cache errors
-- IMPORTANT: Run this in Supabase SQL Editor BEFORE restarting the application

-- Check if column exists first
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'db_customers' 
        AND column_name = 'group'
    ) THEN
        -- Rename the column from 'group' to 'customer_group'
        ALTER TABLE db_customers 
          RENAME COLUMN "group" TO customer_group;
        
        -- Update comment
        COMMENT ON COLUMN db_customers.customer_group IS 'Customer group or category';
        
        RAISE NOTICE 'Column renamed successfully from "group" to "customer_group"';
    ELSE
        RAISE NOTICE 'Column "group" does not exist or already renamed';
    END IF;
END $$;

-- Verification query
SELECT 
    column_name, 
    data_type,
    CASE 
        WHEN column_name = 'customer_group' THEN '✅ Column renamed successfully'
        WHEN column_name = 'group' THEN '⚠️ Old column still exists'
        ELSE ''
    END as status
FROM information_schema.columns 
WHERE table_name = 'db_customers' 
  AND column_name IN ('group', 'customer_group');

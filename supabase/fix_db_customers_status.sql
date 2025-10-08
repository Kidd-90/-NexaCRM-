-- Quick fix: Update all existing db_customers records with correct status values
-- Run this directly in Supabase SQL Editor

-- First, check what statuses currently exist
SELECT DISTINCT status, COUNT(*) as count
FROM db_customers
GROUP BY status;

-- Update invalid statuses to valid DbStatus enum values
UPDATE db_customers
SET status = CASE
    WHEN status IN ('Active', 'InProgress', 'InProgress') THEN 'InProgress'
    WHEN status IN ('Inactive', 'NoAnswer') THEN 'NoAnswer'
    WHEN status IN ('Prospect', 'New') THEN 'New'
    WHEN status IN ('Churned', 'OnHold') THEN 'OnHold'
    WHEN status = 'Completed' THEN 'Completed'
    ELSE 'New'
END
WHERE status NOT IN ('New', 'InProgress', 'NoAnswer', 'Completed', 'OnHold');

-- Verify the update
SELECT DISTINCT status, COUNT(*) as count
FROM db_customers
GROUP BY status
ORDER BY status;

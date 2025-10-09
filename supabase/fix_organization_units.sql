-- =====================================================
-- Fix organization_units: Check and Insert Default Data
-- =====================================================
-- This script fixes the foreign key constraint error by ensuring
-- organization_units table has at least one record

-- 1. Check current organization_units
SELECT * FROM organization_units;

-- 2. Insert default organization unit if none exists
INSERT INTO organization_units (id, name, tenant_code, created_at, updated_at)
VALUES (1, '기본 조직', 'DEFAULT', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- 3. Reset sequence if needed (in case id=1 was manually inserted)
SELECT setval('organization_units_id_seq', COALESCE((SELECT MAX(id) FROM organization_units), 1), true);

-- 4. Verify insertion
SELECT * FROM organization_units;

-- 5. Check biz_companies count
SELECT COUNT(*) as company_count FROM biz_companies;

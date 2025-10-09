-- Migration: Rename org_ prefix to biz_ prefix
-- Date: 2025-10-09
-- Purpose: Rename organization tables to business tables for better clarity in franchise context

-- Step 1: Rename tables (in reverse dependency order to avoid FK conflicts)
ALTER TABLE org_company_team_lists RENAME TO biz_company_team_lists;
ALTER TABLE org_company_branch_lists RENAME TO biz_company_branch_lists;
ALTER TABLE org_branches RENAME TO biz_branches;
ALTER TABLE org_companies RENAME TO biz_companies;

-- Step 2: Rename triggers
ALTER TRIGGER set_timestamp_org_companies ON biz_companies RENAME TO set_timestamp_biz_companies;
ALTER TRIGGER set_timestamp_org_branches ON biz_branches RENAME TO set_timestamp_biz_branches;
ALTER TRIGGER set_timestamp_org_company_branch_lists ON biz_company_branch_lists RENAME TO set_timestamp_biz_company_branch_lists;
ALTER TRIGGER set_timestamp_org_company_team_lists ON biz_company_team_lists RENAME TO set_timestamp_biz_company_team_lists;

-- Step 3: Rename indexes
-- biz_companies indexes
ALTER INDEX idx_org_companies_tenant_unit RENAME TO idx_biz_companies_tenant_unit;

-- biz_branches indexes
ALTER INDEX idx_org_branches_company RENAME TO idx_biz_branches_company;
ALTER INDEX idx_org_branches_tenant_unit RENAME TO idx_biz_branches_tenant_unit;
ALTER INDEX idx_org_branches_manager_cuid RENAME TO idx_biz_branches_manager_cuid;

-- biz_company_branch_lists indexes
ALTER INDEX idx_org_company_branch_lists_tenant RENAME TO idx_biz_company_branch_lists_tenant;
ALTER INDEX idx_org_company_branch_lists_company RENAME TO idx_biz_company_branch_lists_company;
ALTER INDEX idx_org_company_branch_lists_branch RENAME TO idx_biz_company_branch_lists_branch;
ALTER INDEX idx_org_company_branch_lists_manager_cuid RENAME TO idx_biz_company_branch_lists_manager_cuid;

-- biz_company_team_lists indexes
ALTER INDEX idx_org_company_team_lists_tenant RENAME TO idx_biz_company_team_lists_tenant;
ALTER INDEX idx_org_company_team_lists_company RENAME TO idx_biz_company_team_lists_company;
ALTER INDEX idx_org_company_team_lists_branch RENAME TO idx_biz_company_team_lists_branch;
ALTER INDEX idx_org_company_team_lists_manager_cuid RENAME TO idx_biz_company_team_lists_manager_cuid;

-- Step 4: Add comments to document the change
COMMENT ON TABLE biz_companies IS 'Business companies/brands (renamed from org_companies for franchise context)';
COMMENT ON TABLE biz_branches IS 'Business branches/stores (renamed from org_branches for franchise context)';
COMMENT ON TABLE biz_company_branch_lists IS 'Business company-branch summary lists (renamed from org_company_branch_lists)';
COMMENT ON TABLE biz_company_team_lists IS 'Business company-team summary lists (renamed from org_company_team_lists)';

-- Step 5: Verify the migration
DO $$
BEGIN
    -- Check if all tables exist with new names
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'biz_companies') THEN
        RAISE EXCEPTION 'Migration failed: biz_companies table not found';
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'biz_branches') THEN
        RAISE EXCEPTION 'Migration failed: biz_branches table not found';
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'biz_company_branch_lists') THEN
        RAISE EXCEPTION 'Migration failed: biz_company_branch_lists table not found';
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'biz_company_team_lists') THEN
        RAISE EXCEPTION 'Migration failed: biz_company_team_lists table not found';
    END IF;
    
    RAISE NOTICE 'Migration completed successfully: org_ tables renamed to biz_';
END $$;

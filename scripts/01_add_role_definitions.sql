-- ============================================
-- Step 1: role_definitions에 기본 역할 추가
-- ============================================

-- 기존 role_definitions 확인
SELECT 'BEFORE' AS stage, role_code, role_name, description
FROM role_definitions
ORDER BY role_code;

-- 기본 역할 추가 (충돌 시 무시)
INSERT INTO role_definitions (role_code, role_name, description, created_at)
VALUES 
    ('Admin', 'Administrator', 'System administrator with full access', NOW()),
    ('Manager', 'Manager', 'Team manager with management capabilities', NOW()),
    ('Sales', 'Sales Representative', 'Sales team member', NOW()),
    ('User', 'Standard User', 'Basic user with limited access', NOW())
ON CONFLICT (role_code) DO NOTHING;

-- 추가 후 확인
SELECT 'AFTER' AS stage, role_code, role_name, description
FROM role_definitions
ORDER BY role_code;

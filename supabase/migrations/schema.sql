-- Supabase CRM Schema for NexaCRM

-- 1. HELPER FUNCTION for updated_at
-- This trigger function will be used to automatically update the `updated_at` timestamp
CREATE OR REPLACE FUNCTION handle_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;


-- 2. PROFILES TABLE
-- This table stores public user information and is linked to the `auth.users` table.
CREATE TABLE profiles (
  id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  username TEXT UNIQUE,
  full_name TEXT,
  avatar_url TEXT,
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE profiles IS 'Public-facing user profile information.';
COMMENT ON COLUMN profiles.id IS 'Foreign key to auth.users.id.';


-- 3. COMPANIES TABLE
-- Stores information about customer companies.
CREATE TABLE companies (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  website TEXT,
  phone TEXT,
  address TEXT,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE companies IS 'Organizations and customer companies.';


-- 4. CONTACTS TABLE
-- Stores information about individual contacts.
CREATE TABLE contacts (
  id BIGSERIAL PRIMARY KEY,
  first_name TEXT,
  last_name TEXT NOT NULL,
  email TEXT UNIQUE,
  phone TEXT,
  title TEXT,
  company_id BIGINT REFERENCES companies(id) ON DELETE SET NULL,
  company_name TEXT,
  assigned_to UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE contacts IS 'Individual people, usually associated with a company.';


-- 5. DEAL STAGES TABLE
-- Defines the stages in the sales pipeline (e.g., for a Kanban board).
CREATE TABLE deal_stages (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL UNIQUE,
  sort_order INT NOT NULL
);

COMMENT ON TABLE deal_stages IS 'Defines the stages for a sales pipeline.';


-- 6. DEALS TABLE
-- Tracks sales opportunities.
CREATE TABLE deals (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  value NUMERIC,
  company_id BIGINT REFERENCES companies(id) ON DELETE CASCADE,
  company_name TEXT,
  contact_id BIGINT REFERENCES contacts(id) ON DELETE SET NULL,
  contact_name TEXT,
  stage_id INT NOT NULL REFERENCES deal_stages(id),
  assigned_to UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  assigned_to_name TEXT,
  expected_close_date DATE,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE deals IS 'Sales opportunities with value and stage.';


-- 7. ACTIVITIES TABLE
-- Logs interactions like emails, calls, and meetings.
CREATE TABLE activities (
  id BIGSERIAL PRIMARY KEY,
  type TEXT NOT NULL, -- e.g., 'email', 'call', 'meeting'
  notes TEXT,
  activity_date TIMESTAMPTZ NOT NULL,
  contact_id BIGINT REFERENCES contacts(id) ON DELETE CASCADE,
  deal_id BIGINT REFERENCES deals(id) ON DELETE CASCADE,
  user_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE activities IS 'Logs interactions with contacts or deals.';


-- 8. ORGANIZATION STRUCTURE TABLES
CREATE TABLE organization_units (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  parent_id BIGINT REFERENCES organization_units(id) ON DELETE SET NULL,
  tenant_code TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE organization_users (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  unit_id BIGINT REFERENCES organization_units(id) ON DELETE SET NULL,
  role TEXT NOT NULL,
  status TEXT NOT NULL DEFAULT 'pending',
  department TEXT,
  phone_number TEXT,
  registered_at TIMESTAMPTZ DEFAULT NOW(),
  approved_at TIMESTAMPTZ,
  approval_memo TEXT,
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (user_id, unit_id)
);

CREATE TABLE user_roles (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  role_code TEXT NOT NULL,
  assigned_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  assigned_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (user_id, role_code)
);


-- 9. TASKS TABLE
CREATE TABLE tasks (
  id BIGSERIAL PRIMARY KEY,
  title TEXT NOT NULL,
  description TEXT,
  due_date DATE,
  is_completed BOOLEAN DEFAULT FALSE,
  priority TEXT DEFAULT 'Medium',
  assigned_to UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  assigned_to_name TEXT,
  contact_id BIGINT REFERENCES contacts(id) ON DELETE SET NULL,
  deal_id BIGINT REFERENCES deals(id) ON DELETE SET NULL,
  tenant_unit_id BIGINT REFERENCES organization_units(id) ON DELETE SET NULL,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE tasks IS 'User tasks tracked for follow-up and workflow management.';


-- 10. INDEXES FOR PERFORMANCE
CREATE INDEX idx_contacts_email ON contacts(email);
CREATE INDEX idx_deals_stage_id ON deals(stage_id);
CREATE INDEX idx_tasks_assigned_to ON tasks(assigned_to);
CREATE INDEX idx_tasks_tenant_unit_id ON tasks(tenant_unit_id);
CREATE INDEX idx_org_users_user_id ON organization_users(user_id);
CREATE INDEX idx_user_roles_user_id ON user_roles(user_id);


-- 11. TRIGGERS for updated_at
-- Apply the `handle_updated_at` trigger to all tables that have an `updated_at` column.
CREATE TRIGGER on_profiles_updated
  BEFORE UPDATE ON profiles
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_companies_updated
  BEFORE UPDATE ON companies
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_contacts_updated
  BEFORE UPDATE ON contacts
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_deals_updated
  BEFORE UPDATE ON deals
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_tasks_updated
  BEFORE UPDATE ON tasks
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_organization_units_updated
  BEFORE UPDATE ON organization_units
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_organization_users_updated
  BEFORE UPDATE ON organization_users
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_user_roles_updated
  BEFORE UPDATE ON user_roles
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();


-- 9. SEED DATA for deal_stages
-- Insert some default stages for the sales pipeline.
INSERT INTO deal_stages (name, sort_order) VALUES
('Lead', 1),
('Contact Made', 2),
('Needs Analysis', 3),
('Proposal Sent', 4),
('Negotiation', 5),
('Won', 6),
('Lost', 7);

-- End of schema

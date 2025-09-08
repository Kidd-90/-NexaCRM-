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
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  first_name TEXT,
  last_name TEXT NOT NULL,
  email TEXT UNIQUE,
  phone TEXT,
  title TEXT,
  company_id UUID REFERENCES companies(id) ON DELETE SET NULL,
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
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  value NUMERIC,
  company_id UUID REFERENCES companies(id) ON DELETE CASCADE,
  contact_id UUID REFERENCES contacts(id) ON DELETE SET NULL,
  stage_id INT NOT NULL REFERENCES deal_stages(id),
  assigned_to UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  expected_close_date DATE,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE deals IS 'Sales opportunities with value and stage.';


-- 7. ACTIVITIES TABLE
-- Logs interactions like emails, calls, and meetings.
CREATE TABLE activities (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  type TEXT NOT NULL, -- e.g., 'email', 'call', 'meeting'
  notes TEXT,
  activity_date TIMESTAMPTZ NOT NULL,
  contact_id UUID REFERENCES contacts(id) ON DELETE CASCADE,
  deal_id UUID REFERENCES deals(id) ON DELETE CASCADE,
  user_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE activities IS 'Logs interactions with contacts or deals.';


-- 8. TRIGGERS for updated_at
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

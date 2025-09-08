-- Supabase RLS Policies for NexaCRM

-- Helper function to get the current user's ID
CREATE OR REPLACE FUNCTION auth.uid() RETURNS UUID AS $$
  SELECT nullif(current_setting('request.jwt.claims', true)::json->>'sub', '')::uuid;
$$ LANGUAGE SQL STABLE;


-- 1. PROFILES
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view their own profile"
  ON profiles FOR SELECT
  USING (auth.uid() = id);

CREATE POLICY "Users can update their own profile"
  ON profiles FOR UPDATE
  USING (auth.uid() = id)
  WITH CHECK (auth.uid() = id);


-- 2. COMPANIES
ALTER TABLE companies ENABLE ROW LEVEL SECURITY;

-- For a typical CRM, company data is often shared.
-- We'll allow all authenticated users to see companies, but only creators can modify them.
CREATE POLICY "Authenticated users can view companies"
  ON companies FOR SELECT
  USING (auth.role() = 'authenticated');

CREATE POLICY "Users can insert new companies"
  ON companies FOR INSERT
  WITH CHECK (auth.uid() = created_by);

CREATE POLICY "Users can update companies they created"
  ON companies FOR UPDATE
  USING (auth.uid() = created_by)
  WITH CHECK (auth.uid() = created_by);

CREATE POLICY "Users can delete companies they created"
  ON companies FOR DELETE
  USING (auth.uid() = created_by);


-- 3. CONTACTS
ALTER TABLE contacts ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view contacts assigned to them or created by them"
  ON contacts FOR SELECT
  USING (auth.uid() = assigned_to OR auth.uid() = created_by);

CREATE POLICY "Users can insert new contacts"
  ON contacts FOR INSERT
  WITH CHECK (auth.uid() = assigned_to OR auth.uid() = created_by);

CREATE POLICY "Users can update contacts assigned to them"
  ON contacts FOR UPDATE
  USING (auth.uid() = assigned_to)
  WITH CHECK (auth.uid() = assigned_to);

CREATE POLICY "Users can delete contacts assigned to them or created by them"
  ON contacts FOR DELETE
  USING (auth.uid() = assigned_to OR auth.uid() = created_by);


-- 4. DEALS
ALTER TABLE deals ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view deals assigned to them or created by them"
  ON deals FOR SELECT
  USING (auth.uid() = assigned_to OR auth.uid() = created_by);

CREATE POLICY "Users can insert new deals"
  ON deals FOR INSERT
  WITH CHECK (auth.uid() = assigned_to OR auth.uid() = created_by);

CREATE POLICY "Users can update deals assigned to them"
  ON deals FOR UPDATE
  USING (auth.uid() = assigned_to)
  WITH CHECK (auth.uid() = assigned_to);

CREATE POLICY "Users can delete deals assigned to them or created by them"
  ON deals FOR DELETE
  USING (auth.uid() = assigned_to OR auth.uid() = created_by);


-- 5. ACTIVITIES
ALTER TABLE activities ENABLE ROW LEVEL SECURITY;

-- This policy is a bit more complex. It checks if the user has access to the related deal or contact.
-- For simplicity in this example, we'll just check if the user created the activity.
-- A more advanced policy could involve joins.
CREATE POLICY "Users can view their own activities"
  ON activities FOR SELECT
  USING (auth.uid() = user_id);

CREATE POLICY "Users can insert their own activities"
  ON activities FOR INSERT
  WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update their own activities"
  ON activities FOR UPDATE
  USING (auth.uid() = user_id)
  WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can delete their own activities"
  ON activities FOR DELETE
  USING (auth.uid() = user_id);


-- 6. DEAL STAGES
-- This is considered public or lookup data. We can allow read access to all users.
ALTER TABLE deal_stages ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Authenticated users can view deal stages"
  ON deal_stages FOR SELECT
  USING (auth.role() = 'authenticated');

-- By default, INSERT, UPDATE, DELETE are denied unless a policy allows it.
-- This makes the deal_stages table effectively read-only for clients.
-- Only a service role key or direct DB access could change them.

-- End of RLS policies

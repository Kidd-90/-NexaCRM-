-- Add leads support to contacts table and create lead_sources table
-- Migration: 20251014000000_add_leads_support.sql

-- 1. Create lead_sources table for tracking lead origin
CREATE TABLE IF NOT EXISTS lead_sources (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL UNIQUE,
  description TEXT,
  category TEXT, -- 'marketing', 'referral', 'direct', 'partner', etc.
  is_active BOOLEAN DEFAULT true,
  metadata JSONB, -- Additional source-specific data
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

COMMENT ON TABLE lead_sources IS 'Tracks the origin and channel of leads';
COMMENT ON COLUMN lead_sources.category IS 'Source category for grouping and reporting';

-- Trigger for updated_at
CREATE TRIGGER set_timestamp_lead_sources
  BEFORE UPDATE ON lead_sources
  FOR EACH ROW
  EXECUTE FUNCTION handle_updated_at();

-- 2. Extend contacts table with lead-specific fields
ALTER TABLE contacts 
  ADD COLUMN IF NOT EXISTS lead_status TEXT CHECK (lead_status IN ('new', 'contacted', 'qualified', 'converted', 'lost', 'customer')),
  ADD COLUMN IF NOT EXISTS lead_source_id BIGINT REFERENCES lead_sources(id) ON DELETE SET NULL,
  ADD COLUMN IF NOT EXISTS lead_score INT CHECK (lead_score >= 0 AND lead_score <= 100),
  ADD COLUMN IF NOT EXISTS converted_at TIMESTAMPTZ,
  ADD COLUMN IF NOT EXISTS converted_to_customer_id BIGINT,
  ADD COLUMN IF NOT EXISTS lead_notes TEXT,
  ADD COLUMN IF NOT EXISTS last_activity_date TIMESTAMPTZ,
  ADD COLUMN IF NOT EXISTS follow_up_date TIMESTAMPTZ;

COMMENT ON COLUMN contacts.lead_status IS 'Current status of the lead in the sales funnel';
COMMENT ON COLUMN contacts.lead_source_id IS 'Reference to the source that generated this lead';
COMMENT ON COLUMN contacts.lead_score IS 'Lead quality score from 0-100';
COMMENT ON COLUMN contacts.converted_at IS 'Timestamp when lead was converted to customer';
COMMENT ON COLUMN contacts.follow_up_date IS 'Next scheduled follow-up date';

-- 3. Create index for performance
CREATE INDEX IF NOT EXISTS idx_contacts_lead_status ON contacts(lead_status) WHERE lead_status IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_contacts_lead_source ON contacts(lead_source_id) WHERE lead_source_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_contacts_follow_up_date ON contacts(follow_up_date) WHERE follow_up_date IS NOT NULL;

-- 4. Insert default lead sources
INSERT INTO lead_sources (name, description, category) VALUES
  ('Website Form', 'Lead submitted through website contact form', 'direct'),
  ('Social Media - Facebook', 'Lead from Facebook ads or posts', 'marketing'),
  ('Social Media - Instagram', 'Lead from Instagram ads or posts', 'marketing'),
  ('Social Media - LinkedIn', 'Lead from LinkedIn campaigns', 'marketing'),
  ('Google Ads', 'Lead from Google advertising campaigns', 'marketing'),
  ('Email Campaign', 'Lead from email marketing campaigns', 'marketing'),
  ('Referral', 'Lead referred by existing customer', 'referral'),
  ('Partner Referral', 'Lead referred by business partner', 'partner'),
  ('Trade Show', 'Lead met at trade show or conference', 'event'),
  ('Cold Call', 'Lead generated from cold calling', 'direct'),
  ('Webinar', 'Lead registered through webinar', 'event'),
  ('Content Download', 'Lead downloaded whitepaper or content', 'marketing'),
  ('Chat Bot', 'Lead engaged through website chatbot', 'direct'),
  ('Other', 'Other lead source', 'other')
ON CONFLICT (name) DO NOTHING;

-- 5. Create view for lead inbox (new leads)
CREATE OR REPLACE VIEW lead_inbox AS
SELECT 
  c.id,
  c.first_name,
  c.last_name,
  c.email,
  c.phone,
  c.company_name,
  c.lead_status,
  c.lead_score,
  c.follow_up_date,
  c.last_activity_date,
  c.created_at,
  ls.name AS source_name,
  ls.category AS source_category,
  c.assigned_to,
  c.created_by
FROM contacts c
LEFT JOIN lead_sources ls ON c.lead_source_id = ls.id
WHERE c.lead_status IN ('new', 'contacted', 'qualified')
ORDER BY c.created_at DESC;

COMMENT ON VIEW lead_inbox IS 'Active leads that have not been converted to customers';

-- 6. Enable RLS on lead_sources
ALTER TABLE lead_sources ENABLE ROW LEVEL SECURITY;

-- RLS Policy: Everyone can read lead sources
CREATE POLICY "lead_sources_select_all" ON lead_sources
  FOR SELECT
  USING (true);

-- RLS Policy: Only admins can modify lead sources
CREATE POLICY "lead_sources_modify_admin" ON lead_sources
  FOR ALL
  USING (
    EXISTS (
      SELECT 1 FROM user_roles ur
      WHERE ur.user_cuid = (SELECT cuid FROM app_users WHERE auth_user_id = auth.uid())
      AND ur.role_code = 'Admin'
    )
  );

-- 7. Create function to convert lead to customer
CREATE OR REPLACE FUNCTION convert_lead_to_customer(
  p_contact_id BIGINT,
  p_converted_by UUID
)
RETURNS BOOLEAN AS $$
BEGIN
  UPDATE contacts
  SET 
    lead_status = 'converted',
    converted_at = NOW(),
    updated_at = NOW()
  WHERE id = p_contact_id
  AND lead_status IN ('new', 'contacted', 'qualified');
  
  RETURN FOUND;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION convert_lead_to_customer IS 'Converts a lead to customer status';

-- 8. Verification query
SELECT 
  'lead_sources table' AS item,
  COUNT(*) AS count
FROM lead_sources
UNION ALL
SELECT 
  'contacts with lead_status' AS item,
  COUNT(*) AS count
FROM contacts
WHERE lead_status IS NOT NULL;

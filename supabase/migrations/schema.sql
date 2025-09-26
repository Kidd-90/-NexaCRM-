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

-- Ensure UUID helpers are available for tables that rely on UUID identifiers.
CREATE EXTENSION IF NOT EXISTS "pgcrypto";


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
  created_by_name TEXT,
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


-- 8.a ORGANIZATION DIRECTORY TABLES
CREATE TABLE org_companies (
  id BIGSERIAL PRIMARY KEY,
  tenant_unit_id BIGINT NOT NULL REFERENCES organization_units(id) ON DELETE CASCADE,
  code TEXT NOT NULL,
  name TEXT NOT NULL,
  registration_number TEXT,
  phone TEXT,
  email TEXT,
  address TEXT,
  memo TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (tenant_unit_id, code),
  UNIQUE (tenant_unit_id, name)
);

CREATE TABLE org_branches (
  id BIGSERIAL PRIMARY KEY,
  company_id BIGINT NOT NULL REFERENCES org_companies(id) ON DELETE CASCADE,
  tenant_unit_id BIGINT NOT NULL REFERENCES organization_units(id) ON DELETE CASCADE,
  code TEXT NOT NULL,
  name TEXT NOT NULL,
  phone TEXT,
  email TEXT,
  address TEXT,
  manager_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  memo TEXT,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (company_id, code),
  UNIQUE (company_id, name)
);

CREATE TRIGGER set_timestamp_org_companies
  BEFORE UPDATE ON org_companies
  FOR EACH ROW
  EXECUTE FUNCTION handle_updated_at();

CREATE TRIGGER set_timestamp_org_branches
  BEFORE UPDATE ON org_branches
  FOR EACH ROW
  EXECUTE FUNCTION handle_updated_at();

CREATE INDEX idx_org_companies_tenant_unit ON org_companies(tenant_unit_id);
CREATE INDEX idx_org_branches_company ON org_branches(company_id);
CREATE INDEX idx_org_branches_tenant_unit ON org_branches(tenant_unit_id);


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


-- 10. AGENT & TEAM TABLES
CREATE TABLE agents (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  display_name TEXT NOT NULL,
  email TEXT,
  role TEXT NOT NULL DEFAULT 'agent',
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE teams (
  id BIGSERIAL PRIMARY KEY,
  tenant_unit_id BIGINT NOT NULL REFERENCES organization_units(id) ON DELETE CASCADE,
  company_id BIGINT REFERENCES org_companies(id) ON DELETE SET NULL,
  branch_id BIGINT REFERENCES org_branches(id) ON DELETE SET NULL,
  code TEXT NOT NULL UNIQUE,
  name TEXT NOT NULL,
  manager_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  manager_name TEXT,
  member_count INT NOT NULL DEFAULT 0,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  registered_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE team_members (
  id BIGSERIAL PRIMARY KEY,
  team_id BIGINT REFERENCES teams(id) ON DELETE CASCADE,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  company_id BIGINT REFERENCES org_companies(id) ON DELETE SET NULL,
  branch_id BIGINT REFERENCES org_branches(id) ON DELETE SET NULL,
  team_name TEXT,
  role TEXT NOT NULL,
  employee_code TEXT,
  username TEXT NOT NULL,
  full_name TEXT NOT NULL,
  allow_excel_upload BOOLEAN NOT NULL DEFAULT FALSE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  registered_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (team_id, user_id)
);

CREATE TABLE user_directory_entries (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL UNIQUE REFERENCES auth.users(id) ON DELETE CASCADE,
  company_id BIGINT REFERENCES org_companies(id) ON DELETE SET NULL,
  branch_id BIGINT REFERENCES org_branches(id) ON DELETE SET NULL,
  team_id BIGINT REFERENCES teams(id) ON DELETE SET NULL,
  tenant_unit_id BIGINT REFERENCES organization_units(id) ON DELETE SET NULL,
  job_title TEXT,
  employee_number TEXT,
  employment_type TEXT,
  status TEXT NOT NULL DEFAULT 'active',
  hired_on DATE,
  ended_on DATE,
  notes TEXT,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  CHECK (status IN ('active', 'inactive', 'on_leave', 'terminated'))
);

CREATE TRIGGER set_timestamp_teams
  BEFORE UPDATE ON teams
  FOR EACH ROW
  EXECUTE FUNCTION handle_updated_at();

CREATE TRIGGER set_timestamp_team_members
  BEFORE UPDATE ON team_members
  FOR EACH ROW
  EXECUTE FUNCTION handle_updated_at();

CREATE TRIGGER set_timestamp_user_directory_entries
  BEFORE UPDATE ON user_directory_entries
  FOR EACH ROW
  EXECUTE FUNCTION handle_updated_at();

CREATE INDEX idx_teams_tenant_unit ON teams(tenant_unit_id);
CREATE INDEX idx_teams_company ON teams(company_id);
CREATE INDEX idx_team_members_user ON team_members(user_id);
CREATE INDEX idx_team_members_company ON team_members(company_id);
CREATE INDEX idx_user_directory_company ON user_directory_entries(company_id);
CREATE INDEX idx_user_directory_tenant_unit ON user_directory_entries(tenant_unit_id);

CREATE INDEX idx_teams_active ON teams(is_active);
CREATE INDEX idx_team_members_team_id ON team_members(team_id);


-- 11. SALES MANAGEMENT TABLES
CREATE TABLE sales_appointments (
  id BIGSERIAL PRIMARY KEY,
  title TEXT NOT NULL,
  description TEXT,
  start_datetime TIMESTAMPTZ NOT NULL,
  end_datetime TIMESTAMPTZ NOT NULL,
  contact_id BIGINT REFERENCES contacts(id) ON DELETE SET NULL,
  contact_name TEXT,
  contact_company TEXT,
  type TEXT NOT NULL,
  status TEXT NOT NULL,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  location TEXT,
  notes TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE consultation_notes (
  id BIGSERIAL PRIMARY KEY,
  contact_id BIGINT REFERENCES contacts(id) ON DELETE SET NULL,
  contact_name TEXT,
  title TEXT NOT NULL,
  content TEXT NOT NULL,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  tags TEXT,
  priority TEXT NOT NULL DEFAULT 'Medium',
  is_follow_up_required BOOLEAN NOT NULL DEFAULT FALSE,
  follow_up_date TIMESTAMPTZ,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_sales_appointments_user ON sales_appointments(user_id);
CREATE INDEX idx_sales_appointments_start ON sales_appointments(start_datetime);
CREATE INDEX idx_consultation_notes_user ON consultation_notes(user_id);
CREATE INDEX idx_consultation_notes_contact ON consultation_notes(contact_id);


-- 12. MARKETING CAMPAIGN TABLES
CREATE TABLE marketing_campaigns (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  type TEXT,
  status TEXT NOT NULL DEFAULT 'Draft',
  start_date DATE,
  end_date DATE,
  budget NUMERIC DEFAULT 0,
  roi NUMERIC DEFAULT 0,
  owner_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_marketing_campaigns_status ON marketing_campaigns(status);
CREATE INDEX idx_marketing_campaigns_start ON marketing_campaigns(start_date);


-- 13. EMAIL TEMPLATE TABLES
CREATE TABLE email_templates (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  subject TEXT,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE email_blocks (
  id BIGSERIAL PRIMARY KEY,
  template_id UUID NOT NULL REFERENCES email_templates(id) ON DELETE CASCADE,
  block_order INT NOT NULL,
  block_type TEXT NOT NULL,
  content TEXT NOT NULL
);

CREATE INDEX idx_email_blocks_template ON email_blocks(template_id);


-- 14. INDEXES FOR PERFORMANCE
CREATE INDEX idx_contacts_email ON contacts(email);
CREATE INDEX idx_deals_stage_id ON deals(stage_id);
CREATE INDEX idx_tasks_assigned_to ON tasks(assigned_to);
CREATE INDEX idx_tasks_tenant_unit_id ON tasks(tenant_unit_id);
CREATE INDEX idx_org_users_user_id ON organization_users(user_id);
CREATE INDEX idx_user_roles_user_id ON user_roles(user_id);


-- 15. SUPPORT & SERVICE TABLES
CREATE TABLE support_tickets (
  id BIGSERIAL PRIMARY KEY,
  subject TEXT NOT NULL,
  description TEXT,
  status TEXT NOT NULL DEFAULT 'Open',
  priority TEXT NOT NULL DEFAULT 'Medium',
  customer_id BIGINT REFERENCES contacts(id) ON DELETE SET NULL,
  customer_name TEXT,
  agent_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  agent_name TEXT,
  category TEXT,
  tenant_unit_id BIGINT REFERENCES organization_units(id) ON DELETE SET NULL,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  last_reply_at TIMESTAMPTZ
);

CREATE TABLE ticket_messages (
  id BIGSERIAL PRIMARY KEY,
  ticket_id BIGINT NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
  sender_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  sender_name TEXT,
  message TEXT NOT NULL,
  is_internal BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_support_tickets_status ON support_tickets(status);
CREATE INDEX idx_support_tickets_agent_id ON support_tickets(agent_id);
CREATE INDEX idx_support_tickets_tenant_unit ON support_tickets(tenant_unit_id);
CREATE INDEX idx_ticket_messages_ticket_id ON ticket_messages(ticket_id);


-- 16. NOTIFICATION FEED TABLES
CREATE TABLE notification_feed (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  title TEXT NOT NULL,
  message TEXT,
  type TEXT DEFAULT 'info',
  is_read BOOLEAN DEFAULT FALSE,
  metadata JSONB,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE notification_settings (
  user_id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  new_lead_created BOOLEAN DEFAULT TRUE,
  lead_status_updated BOOLEAN DEFAULT TRUE,
  deal_stage_changed BOOLEAN DEFAULT TRUE,
  deal_value_updated BOOLEAN DEFAULT TRUE,
  new_task_assigned BOOLEAN DEFAULT TRUE,
  task_due_date_reminder BOOLEAN DEFAULT TRUE,
  email_notifications BOOLEAN DEFAULT TRUE,
  in_app_notifications BOOLEAN DEFAULT TRUE,
  push_notifications BOOLEAN DEFAULT FALSE,
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_notification_feed_user_id ON notification_feed(user_id);
CREATE INDEX idx_notification_feed_is_read ON notification_feed(is_read);


-- 17. SMS & COMMUNICATION TABLES
CREATE TABLE sms_settings (
  user_id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  provider_api_key TEXT,
  provider_api_secret TEXT,
  sender_id TEXT,
  default_template TEXT,
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE sms_sender_numbers (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  number TEXT NOT NULL,
  label TEXT,
  is_default BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE(user_id, number)
);

CREATE TABLE sms_templates (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  template_code TEXT,
  content TEXT NOT NULL,
  is_default BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE sms_history (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  recipient TEXT NOT NULL,
  message TEXT NOT NULL,
  sent_at TIMESTAMPTZ DEFAULT NOW(),
  status TEXT NOT NULL,
  sender_number TEXT,
  recipient_name TEXT,
  attachments JSONB,
  error_message TEXT,
  metadata JSONB
);

CREATE TABLE sms_schedules (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  scheduled_at TIMESTAMPTZ NOT NULL,
  payload_json JSONB NOT NULL,
  is_cancelled BOOLEAN DEFAULT FALSE,
  status TEXT DEFAULT 'scheduled',
  created_at TIMESTAMPTZ DEFAULT NOW(),
  dispatched_at TIMESTAMPTZ
);

CREATE INDEX idx_sms_sender_numbers_user ON sms_sender_numbers(user_id);
CREATE INDEX idx_sms_templates_user ON sms_templates(user_id);
CREATE INDEX idx_sms_history_user ON sms_history(user_id);
CREATE INDEX idx_sms_schedules_user ON sms_schedules(user_id);
CREATE INDEX idx_sms_schedules_time ON sms_schedules(scheduled_at);


-- 18. AUDIT & INTEGRATION TABLES
CREATE TABLE audit_logs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  actor_id UUID REFERENCES auth.users(id) ON DELETE SET NULL,
  action TEXT NOT NULL,
  entity_type TEXT NOT NULL,
  entity_id TEXT,
  payload_json JSONB,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE integration_events (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  event_type TEXT NOT NULL,
  payload_json JSONB,
  status TEXT DEFAULT 'pending',
  published_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_audit_logs_actor ON audit_logs(actor_id);
CREATE INDEX idx_integration_events_type ON integration_events(event_type);
CREATE INDEX idx_integration_events_status ON integration_events(status);


-- 19. ANALYTICS & REPORTING TABLES
CREATE TABLE report_definitions (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  name TEXT NOT NULL,
  selected_fields TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[],
  filters_json JSONB,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (user_id, name)
);

CREATE TABLE report_snapshots (
  id BIGSERIAL PRIMARY KEY,
  definition_id BIGINT REFERENCES report_definitions(id) ON DELETE CASCADE,
  generated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  payload_json JSONB NOT NULL,
  format TEXT NOT NULL DEFAULT 'json',
  metrics_summary JSONB,
  created_by UUID REFERENCES auth.users(id) ON DELETE SET NULL
);

CREATE INDEX idx_report_snapshots_definition ON report_snapshots(definition_id);
CREATE INDEX idx_report_snapshots_created_by ON report_snapshots(created_by);
CREATE INDEX idx_report_snapshots_generated_at ON report_snapshots(generated_at DESC);

CREATE TABLE statistics_daily (
  id BIGSERIAL PRIMARY KEY,
  metric_date DATE NOT NULL,
  tenant_unit_id BIGINT REFERENCES organization_units(id) ON DELETE SET NULL,
  total_members INT NOT NULL DEFAULT 0,
  total_logins INT NOT NULL DEFAULT 0,
  total_downloads INT NOT NULL DEFAULT 0,
  active_users INT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW(),
  UNIQUE (metric_date, tenant_unit_id)
);

CREATE INDEX idx_statistics_daily_metric_date ON statistics_daily(metric_date);
CREATE INDEX idx_statistics_daily_tenant ON statistics_daily(tenant_unit_id);


-- Ensure realtime payloads include previous values for proper diffing.
ALTER TABLE tasks REPLICA IDENTITY FULL;
ALTER TABLE support_tickets REPLICA IDENTITY FULL;
ALTER TABLE notification_feed REPLICA IDENTITY FULL;
ALTER TABLE sms_schedules REPLICA IDENTITY FULL;
ALTER TABLE sms_history REPLICA IDENTITY FULL;


-- 20. TRIGGERS for updated_at
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

CREATE TRIGGER on_agents_updated
  BEFORE UPDATE ON agents
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_teams_updated
  BEFORE UPDATE ON teams
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_team_members_updated
  BEFORE UPDATE ON team_members
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_sales_appointments_updated
  BEFORE UPDATE ON sales_appointments
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_consultation_notes_updated
  BEFORE UPDATE ON consultation_notes
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_marketing_campaigns_updated
  BEFORE UPDATE ON marketing_campaigns
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_email_templates_updated
  BEFORE UPDATE ON email_templates
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

CREATE TRIGGER on_report_definitions_updated
  BEFORE UPDATE ON report_definitions
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_report_snapshots_updated
  BEFORE UPDATE ON report_snapshots
  FOR EACH ROW EXECUTE PROCEDURE handle_updated_at();

CREATE TRIGGER on_statistics_daily_updated
  BEFORE UPDATE ON statistics_daily
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

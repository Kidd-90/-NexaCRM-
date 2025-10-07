-- Supabase RLS Policies for NexaCRM

-- Helper function to get the current user's ID
CREATE OR REPLACE FUNCTION auth.uid() RETURNS UUID AS $$
  SELECT nullif(current_setting('request.jwt.claims', true)::json->>'sub', '')::uuid;
$$ LANGUAGE SQL STABLE;


CREATE OR REPLACE FUNCTION public.user_has_role(role_name text)
RETURNS boolean AS $$
  SELECT EXISTS (
    SELECT 1 FROM user_roles
    WHERE user_id = auth.uid()
      AND lower(role_code) = lower(role_name)
  );
$$ LANGUAGE sql STABLE;


CREATE OR REPLACE FUNCTION public.user_is_approved()
RETURNS boolean AS $$
  SELECT EXISTS (
    SELECT 1 FROM organization_users
    WHERE user_id = auth.uid()
      AND status = 'approved'
  );
$$ LANGUAGE sql STABLE;


-- 0. APP USERS & USER INFOS
ALTER TABLE app_users ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_infos ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view their app user mapping"
  ON app_users FOR SELECT
  USING (auth.uid() = auth_user_id);

CREATE POLICY "Service role manages app users"
  ON app_users FOR ALL
  USING (auth.role() = 'service_role')
  WITH CHECK (auth.role() = 'service_role');

CREATE POLICY "Users can view their user info"
  ON user_infos FOR SELECT
  USING (EXISTS (
    SELECT 1 FROM app_users
    WHERE app_users.cuid = user_infos.user_cuid
      AND app_users.auth_user_id = auth.uid()
  ));

CREATE POLICY "Users can update their user info"
  ON user_infos FOR UPDATE
  USING (EXISTS (
    SELECT 1 FROM app_users
    WHERE app_users.cuid = user_infos.user_cuid
      AND app_users.auth_user_id = auth.uid()
  ))
  WITH CHECK (EXISTS (
    SELECT 1 FROM app_users
    WHERE app_users.cuid = user_infos.user_cuid
      AND app_users.auth_user_id = auth.uid()
  ));

CREATE POLICY "Service role manages user infos"
  ON user_infos FOR ALL
  USING (auth.role() = 'service_role')
  WITH CHECK (auth.role() = 'service_role');


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
  USING ((auth.role() = 'authenticated' AND public.user_is_approved()) OR public.user_has_role('admin'));

CREATE POLICY "Users can insert new companies"
  ON companies FOR INSERT
  WITH CHECK ((auth.uid() = created_by AND public.user_is_approved()) OR public.user_has_role('admin'));

CREATE POLICY "Users can update companies they created"
  ON companies FOR UPDATE
  USING ((auth.uid() = created_by AND public.user_is_approved()) OR public.user_has_role('admin'))
  WITH CHECK ((auth.uid() = created_by AND public.user_is_approved()) OR public.user_has_role('admin'));

CREATE POLICY "Users can delete companies they created"
  ON companies FOR DELETE
  USING ((auth.uid() = created_by AND public.user_is_approved()) OR public.user_has_role('admin'));


-- 3. CONTACTS
ALTER TABLE contacts ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view contacts assigned to them or created by them"
  ON contacts FOR SELECT
  USING ((auth.uid() = assigned_to OR auth.uid() = created_by OR public.user_has_role('admin')) AND public.user_is_approved());

CREATE POLICY "Users can insert new contacts"
  ON contacts FOR INSERT
  WITH CHECK ((auth.uid() = assigned_to OR auth.uid() = created_by OR public.user_has_role('admin')) AND public.user_is_approved());

CREATE POLICY "Users can update contacts assigned to them"
  ON contacts FOR UPDATE
  USING ((auth.uid() = assigned_to AND public.user_is_approved()) OR public.user_has_role('admin'))
  WITH CHECK ((auth.uid() = assigned_to AND public.user_is_approved()) OR public.user_has_role('admin'));

CREATE POLICY "Users can delete contacts assigned to them or created by them"
  ON contacts FOR DELETE
  USING ((auth.uid() = assigned_to OR auth.uid() = created_by OR public.user_has_role('admin')) AND public.user_is_approved());


-- 4. DEALS
ALTER TABLE deals ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view deals assigned to them or created by them"
  ON deals FOR SELECT
  USING ((auth.uid() = assigned_to OR auth.uid() = created_by OR public.user_has_role('admin')) AND public.user_is_approved());

CREATE POLICY "Users can insert new deals"
  ON deals FOR INSERT
  WITH CHECK ((auth.uid() = assigned_to OR auth.uid() = created_by OR public.user_has_role('admin')) AND public.user_is_approved());

CREATE POLICY "Users can update deals assigned to them"
  ON deals FOR UPDATE
  USING ((auth.uid() = assigned_to AND public.user_is_approved()) OR public.user_has_role('admin'))
  WITH CHECK ((auth.uid() = assigned_to AND public.user_is_approved()) OR public.user_has_role('admin'));

CREATE POLICY "Users can delete deals assigned to them or created by them"
  ON deals FOR DELETE
  USING ((auth.uid() = assigned_to OR auth.uid() = created_by OR public.user_has_role('admin')) AND public.user_is_approved());


-- 5. ACTIVITIES
ALTER TABLE activities ENABLE ROW LEVEL SECURITY;

-- This policy is a bit more complex. It checks if the user has access to the related deal or contact.
-- For simplicity in this example, we'll just check if the user created the activity.
-- A more advanced policy could involve joins.
CREATE POLICY "Users can view their own activities"
  ON activities FOR SELECT
  USING (auth.uid() = user_id OR public.user_has_role('admin'));

CREATE POLICY "Users can insert their own activities"
  ON activities FOR INSERT
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

CREATE POLICY "Users can update their own activities"
  ON activities FOR UPDATE
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

CREATE POLICY "Users can delete their own activities"
  ON activities FOR DELETE
  USING (auth.uid() = user_id OR public.user_has_role('admin'));


-- 6. DEAL STAGES
-- This is considered public or lookup data. We can allow read access to all users.
ALTER TABLE deal_stages ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Authenticated users can view deal stages"
  ON deal_stages FOR SELECT
  USING (auth.role() = 'authenticated');

-- By default, INSERT, UPDATE, DELETE are denied unless a policy allows it.
-- This makes the deal_stages table effectively read-only for clients.
-- Only a service role key or direct DB access could change them.

-- 7. TASKS
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view tasks scoped to them"
  ON tasks FOR SELECT
  USING (
    public.user_has_role('admin')
    OR auth.uid() = assigned_to
    OR auth.uid() = created_by
    OR (
      tenant_unit_id IS NOT NULL AND EXISTS (
        SELECT 1 FROM organization_users
        WHERE organization_users.user_id = auth.uid()
          AND organization_users.unit_id = tasks.tenant_unit_id
          AND organization_users.status = 'approved'
      )
    )
  );

CREATE POLICY "Users can insert tasks they create"
  ON tasks FOR INSERT
  WITH CHECK (public.user_has_role('admin') OR auth.uid() = created_by);

CREATE POLICY "Users can update tasks assigned or created"
  ON tasks FOR UPDATE
  USING (public.user_has_role('admin') OR auth.uid() = assigned_to OR auth.uid() = created_by)
  WITH CHECK (public.user_has_role('admin') OR auth.uid() = assigned_to OR auth.uid() = created_by);

CREATE POLICY "Users can delete tasks they created"
  ON tasks FOR DELETE
  USING (public.user_has_role('admin') OR auth.uid() = created_by);


-- 8.a ORGANIZATION DIRECTORY RLS
ALTER TABLE org_companies ENABLE ROW LEVEL SECURITY;
ALTER TABLE org_branches ENABLE ROW LEVEL SECURITY;
ALTER TABLE org_company_branch_lists ENABLE ROW LEVEL SECURITY;
ALTER TABLE teams ENABLE ROW LEVEL SECURITY;
ALTER TABLE team_members ENABLE ROW LEVEL SECURITY;
ALTER TABLE org_company_team_lists ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_directory_entries ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Approved users can view org companies"
  ON org_companies FOR SELECT
  USING (
    public.user_has_role('admin')
    OR EXISTS (
      SELECT 1 FROM organization_users
      WHERE organization_users.user_id = auth.uid()
        AND organization_users.unit_id = org_companies.tenant_unit_id
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage org companies"
  ON org_companies FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

CREATE POLICY "Approved users can view org branches"
  ON org_branches FOR SELECT
  USING (
    public.user_has_role('admin')
    OR EXISTS (
      SELECT 1 FROM organization_users
      WHERE organization_users.user_id = auth.uid()
        AND organization_users.unit_id = org_branches.tenant_unit_id
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage org branches"
  ON org_branches FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

CREATE POLICY "Approved users can view org company branch lists"
  ON org_company_branch_lists FOR SELECT
  USING (
    public.user_has_role('admin')
    OR EXISTS (
      SELECT 1 FROM organization_users
      WHERE organization_users.user_id = auth.uid()
        AND organization_users.unit_id = org_company_branch_lists.tenant_unit_id
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage org company branch lists"
  ON org_company_branch_lists FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

CREATE POLICY "Approved users can view teams"
  ON teams FOR SELECT
  USING (
    public.user_has_role('admin')
    OR EXISTS (
      SELECT 1 FROM organization_users
      WHERE organization_users.user_id = auth.uid()
        AND organization_users.unit_id = teams.tenant_unit_id
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage teams"
  ON teams FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

CREATE POLICY "Users can view their team membership"
  ON team_members FOR SELECT
  USING (
    public.user_has_role('admin')
    OR (user_id IS NOT NULL AND user_id = auth.uid())
    OR EXISTS (
      SELECT 1
      FROM teams
      JOIN organization_users ON organization_users.unit_id = teams.tenant_unit_id
      WHERE teams.id = team_members.team_id
        AND organization_users.user_id = auth.uid()
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage team members"
  ON team_members FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

CREATE POLICY "Approved users can view company team lists"
  ON org_company_team_lists FOR SELECT
  USING (
    public.user_has_role('admin')
    OR EXISTS (
      SELECT 1 FROM organization_users
      WHERE organization_users.user_id = auth.uid()
        AND organization_users.unit_id = org_company_team_lists.tenant_unit_id
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage company team lists"
  ON org_company_team_lists FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

CREATE POLICY "Users can view their directory entry"
  ON user_directory_entries FOR SELECT
  USING (auth.uid() = user_id OR public.user_has_role('admin'));

CREATE POLICY "Admins manage user directory"
  ON user_directory_entries FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));


-- SUPPORT & SERVICE RLS
ALTER TABLE support_tickets ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view relevant support tickets"
  ON support_tickets FOR SELECT
  USING (
    public.user_has_role('admin')
    OR auth.uid() = agent_id
    OR auth.uid() = created_by
    OR (
      tenant_unit_id IS NOT NULL AND EXISTS (
        SELECT 1 FROM organization_users
        WHERE organization_users.user_id = auth.uid()
          AND organization_users.unit_id = support_tickets.tenant_unit_id
          AND organization_users.status = 'approved'
      )
    )
  );

CREATE POLICY "Users can insert support tickets they create"
  ON support_tickets FOR INSERT
  WITH CHECK (public.user_has_role('admin') OR auth.uid() = created_by);

CREATE POLICY "Users can update assigned support tickets"
  ON support_tickets FOR UPDATE
  USING (public.user_has_role('admin') OR auth.uid() = agent_id OR auth.uid() = created_by)
  WITH CHECK (public.user_has_role('admin') OR auth.uid() = agent_id OR auth.uid() = created_by);

CREATE POLICY "Users can delete support tickets they created"
  ON support_tickets FOR DELETE
  USING (public.user_has_role('admin') OR auth.uid() = created_by);

ALTER TABLE ticket_messages ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view messages for accessible tickets"
  ON ticket_messages FOR SELECT
  USING (
    public.user_has_role('admin')
    OR EXISTS (
      SELECT 1 FROM support_tickets
      WHERE support_tickets.id = ticket_messages.ticket_id
        AND (
          public.user_has_role('admin')
          OR auth.uid() = support_tickets.agent_id
          OR auth.uid() = support_tickets.created_by
        )
    )
  );

CREATE POLICY "Users can insert ticket messages they author"
  ON ticket_messages FOR INSERT
  WITH CHECK (public.user_has_role('admin') OR auth.uid() = sender_id);


-- NOTIFICATION FEED RLS
ALTER TABLE notification_feed ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view their notifications"
  ON notification_feed FOR SELECT
  USING (auth.uid() = user_id OR public.user_has_role('admin'));

CREATE POLICY "Users can manage their notifications"
  ON notification_feed FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

ALTER TABLE notification_settings ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users manage their notification settings"
  ON notification_settings FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));


-- SMS & COMMUNICATION RLS
ALTER TABLE sms_settings ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users manage their SMS settings"
  ON sms_settings FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

ALTER TABLE sms_sender_numbers ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users manage sender numbers"
  ON sms_sender_numbers FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

ALTER TABLE sms_templates ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users manage SMS templates"
  ON sms_templates FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

ALTER TABLE sms_history ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view their SMS history"
  ON sms_history FOR SELECT
  USING (auth.uid() = user_id OR public.user_has_role('admin'));

CREATE POLICY "Users can insert their SMS history"
  ON sms_history FOR INSERT
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

ALTER TABLE sms_schedules ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users manage their SMS schedules"
  ON sms_schedules FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));


-- AUDIT & INTEGRATION RLS
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can insert their audit logs"
  ON audit_logs FOR INSERT
  WITH CHECK (public.user_has_role('admin') OR auth.uid() = actor_id);

CREATE POLICY "Admins can view audit logs"
  ON audit_logs FOR SELECT
  USING (public.user_has_role('admin'));

ALTER TABLE integration_events ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Authenticated users can publish integration events"
  ON integration_events FOR INSERT
  WITH CHECK (public.user_has_role('admin') OR auth.role() = 'authenticated');

CREATE POLICY "Admins can view integration events"
  ON integration_events FOR SELECT
  USING (public.user_has_role('admin'));

CREATE POLICY "Admins can update integration events"
  ON integration_events FOR UPDATE
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));


-- 8. ORGANIZATION TABLES
ALTER TABLE organization_units ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Approved users can view organization units"
  ON organization_units FOR SELECT
  USING (public.user_is_approved() OR public.user_has_role('admin'));

CREATE POLICY "Admins manage organization units"
  ON organization_units FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

ALTER TABLE organization_users ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view their organization membership"
  ON organization_users FOR SELECT
  USING (user_id = auth.uid() OR public.user_has_role('admin'));

CREATE POLICY "Admins manage organization users"
  ON organization_users FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));

ALTER TABLE user_roles ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can read their roles"
  ON user_roles FOR SELECT
  USING (user_id = auth.uid() OR public.user_has_role('admin'));

CREATE POLICY "Admins manage user roles"
  ON user_roles FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));


-- ANALYTICS & REPORTING RLS
ALTER TABLE report_definitions ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users manage their report definitions"
  ON report_definitions FOR ALL
  USING (auth.uid() = user_id OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = user_id OR public.user_has_role('admin'));

ALTER TABLE report_snapshots ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users read their report snapshots"
  ON report_snapshots FOR SELECT
  USING (
    public.user_has_role('admin')
    OR (created_by IS NOT NULL AND auth.uid() = created_by)
    OR (
      definition_id IS NOT NULL AND EXISTS (
        SELECT 1 FROM report_definitions
        WHERE report_definitions.id = report_snapshots.definition_id
          AND report_definitions.user_id = auth.uid()
      )
    )
  );

CREATE POLICY "Users insert report snapshots"
  ON report_snapshots FOR INSERT
  WITH CHECK (
    public.user_has_role('admin')
    OR (created_by IS NOT NULL AND auth.uid() = created_by)
    OR (
      definition_id IS NOT NULL AND EXISTS (
        SELECT 1 FROM report_definitions
        WHERE report_definitions.id = report_snapshots.definition_id
          AND report_definitions.user_id = auth.uid()
      )
    )
  );

CREATE POLICY "Users update their report snapshots"
  ON report_snapshots FOR UPDATE
  USING (auth.uid() = created_by OR public.user_has_role('admin'))
  WITH CHECK (auth.uid() = created_by OR public.user_has_role('admin'));

CREATE POLICY "Users delete their report snapshots"
  ON report_snapshots FOR DELETE
  USING (auth.uid() = created_by OR public.user_has_role('admin'));

ALTER TABLE statistics_daily ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Approved users can view statistics"
  ON statistics_daily FOR SELECT
  USING (
    public.user_has_role('admin')
    OR tenant_unit_id IS NULL
    OR EXISTS (
      SELECT 1 FROM organization_users
      WHERE organization_users.user_id = auth.uid()
        AND organization_users.unit_id = statistics_daily.tenant_unit_id
        AND organization_users.status = 'approved'
    )
  );

CREATE POLICY "Admins manage statistics"
  ON statistics_daily FOR ALL
  USING (public.user_has_role('admin'))
  WITH CHECK (public.user_has_role('admin'));


-- 19. CUSTOMER NOTICES
ALTER TABLE customer_notices ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Enable read access for all users"
  ON customer_notices
  FOR SELECT
  TO public
  USING (true);

CREATE POLICY "Enable insert for authenticated users only"
  ON customer_notices
  FOR INSERT
  TO authenticated
  WITH CHECK (true);

CREATE POLICY "Enable update for authenticated users only"
  ON customer_notices
  FOR UPDATE
  TO authenticated
  USING (true)
  WITH CHECK (true);

CREATE POLICY "Enable delete for authenticated users only"
  ON customer_notices
  FOR DELETE
  TO authenticated
  USING (true);


-- End of RLS policies

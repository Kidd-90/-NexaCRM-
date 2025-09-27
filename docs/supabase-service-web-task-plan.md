# Supabase Integration Task Plan (Service vs Web)

This plan is derived from `supabase/SUPABASE_INTEGRATION_GUIDE.md` and organizes the required work items by functionality for both the **service** (.NET backend/API) and **web** (Blazor WebApp) projects. Use it as a checklist to drive the Supabase migration roadmap.

## Environment & Secrets Management
- **Service**
  - [ ] Configure environment variables (`SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY`, `SUPABASE_JWT_SECRET`) via secure secret storage (Azure Key Vault or equivalent).
  - [ ] Implement configuration binding to expose Supabase settings through strongly-typed options classes.
  - [x] Added `SupabaseServerOptions` and DI extensions to bind and validate server secrets from configuration.
- **Web**
  - [ ] Inject `SUPABASE_URL` and `SUPABASE_ANON_KEY` into `appsettings.{Environment}.json` or user secrets.
  - [ ] Validate build-time transforms to prevent secrets from being committed.
  - [x] Added `SupabaseClientOptions` binding and sample placeholders in `wwwroot/appsettings.json` for WASM consumption.

## Authentication & Session Management
- **Service**
  - [ ] Replace existing auth middleware/providers with Supabase Auth verification using the `service_role` key when necessary.
  - [ ] Provide API endpoints to issue, refresh, and revoke Supabase-backed sessions for internal services.
- **Web**
  - [x] Adopt the shared `SupabaseAuthenticationStateProvider` that uses Supabase Auth client APIs.
  - [x] Handle session persistence and automatic token refresh via Supabase client options (`AutoRefreshToken`).

## Data Access (Contacts, Deals, Tasks)
- **Service**
  - [ ] Update domain services (`IContactService`, `IDealService`, `ITaskService`) to execute CRUD against Supabase PostgREST using `SUPABASE_SERVICE_ROLE_KEY` for elevated access when RLS applies.
  - [ ] Implement repository abstractions using `Postgrest.Table<T>` for CRUD and filters.
- **Web**
  - [ ] Update data service clients to consume Supabase REST endpoints or direct client SDK queries for Contacts, Deals, and Tasks.
  - [ ] Ensure UI components react to Supabase DTO changes (e.g., property casing, nullable columns).

## Organization & Role Management
- **Service**
  - [ ] Integrate `organization_users` and `user_roles` tables to enforce row-level security decisions.
  - [ ] Expose APIs to manage memberships and role assignments consistent with Supabase schemas.
- **Web**
  - [ ] Update administrative UI to display and manage organization membership sourced from Supabase tables.
  - [ ] Align client-side authorization checks with Supabase role claims.

## Realtime Subscriptions (Tickets, Notifications)
- **Service**
  - [ ] Provide server-side event handlers or SignalR bridges if backend workflows must respond to Supabase realtime messages.
  - [ ] Document fallback logic for when realtime channels degrade.
- **Web**
  - [ ] Connect support tickets, task boards, and notifications UI to Supabase realtime channels (`Realtime` property of the client).
  - [ ] Handle reconnection and local cache updates on realtime events.

## Scheduled & Edge Functions
- **Service**
  - [ ] Migrate scheduled SMS/email dispatchers to Supabase Edge Functions with cron triggers.
  - [ ] Secure function endpoints with JWT secrets and ensure observability (logging, retries).
- **Web**
  - [ ] Adjust UX to reflect Edge Function job statuses (e.g., show scheduled send confirmations).
  - [ ] Surface user-friendly error handling for failures reported by Edge Functions.

## Storage Buckets & Assets
- **Service**
  - [ ] Define storage bucket conventions for tickets, campaigns, and private assets.
  - [ ] Implement upload/download helpers leveraging Supabase Storage with service-role access for secure operations.
- **Web**
  - [ ] Integrate client-side uploads for ticket attachments and marketing assets using the anon key and proper ACLs.
  - [ ] Provide UI feedback for storage permissions (public vs private buckets).

## Monitoring & Operations
- **Service**
  - [x] Feed Supabase metrics (query latency, connection counts, error rates) into existing monitoring dashboards. (SupabaseMonitoringService → Grafana)
  - [x] Establish backup, PITR policies, and disaster recovery runbooks aligned with Supabase capabilities. (`supabase/BACKUP_AND_RECOVERY.md`)
- **Web**
  - [ ] Surface user-facing alerts when Supabase availability degrades (e.g., banner notifications).
  - [ ] Instrument client metrics to detect auth or realtime failures.

## Migration & Verification Steps
- **Service**
  - [ ] Automate deployment of `schema.sql` and `rls.sql` via CI using the Supabase CLI.
  - [ ] Write integration tests that validate Supabase RLS enforcement through service APIs.
- **Web**
  - [ ] Update integration tests/end-to-end flows to cover Supabase-backed features.
  - [ ] Ensure build pipelines perform `dotnet restore` after adding Supabase/PostgREST packages.

## Phase Alignment (from Supabase Roadmap)
- **Phase 1 (Auth & Core Data – 2024-07)**
  - Service: Complete auth replacement, migrate contact/deal/task CRUD, connect org/role tables.
  - Web: Implement Supabase Auth provider, refactor CRUD data flows, update UI for role-driven permissions.
- **Phase 2 (Realtime & Integrations – 2024-08)**
  - Service: ✅ Link realtime handlers, move scheduled jobs to Edge Functions, verify integration event flows.
  - Web: Subscribe to realtime updates in support/ticket modules, display job statuses, handle event-driven UI updates.
- **Phase 3 (Analytics & Operations – 2024-09)**
  - Service: ✅ Build ETL pipelines for analytics tables, integrate Supabase monitoring, apply retention policies.
  - Web: ✅ Surface analytics sourced from new tables, provide SLA monitoring dashboards where required.


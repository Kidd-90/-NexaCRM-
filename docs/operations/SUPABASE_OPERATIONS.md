# Supabase Operations Playbook

This document centralizes the operational ownership for the NexaCRM Supabase environment.

## 1. Automation Inventory

| Automation | Purpose | Trigger | Owner |
| --- | --- | --- | --- |
| `ops-supabase-backup.yml` (GitHub Actions) | Verifies daily backup freshness and alerts if `updated_at` &gt; 26h. | 02:30 UTC daily | Platform Eng. |
| `Supabase-PITR-Drill` (Azure Automation) | Executes quarterly point-in-time recovery rehearsal. | Quarterly schedule | Platform Eng. |
| `supabase-metric-exporter` (Azure Container Apps) | Calls the Supabase metrics API and publishes to Grafana Cloud via OTLP. | Every 5 minutes | Observability |
| `scheduled-dispatcher` (Supabase Edge Function) | Processes queued SMS/email jobs using cron triggers. | Every minute | Messaging Team |

## 2. Secrets & Configuration

| Name | Location | Description |
| --- | --- | --- |
| `SUPABASE_SERVICE_ROLE_KEY` | Azure Key Vault `nexa-ops` | Privileged key used by monitoring & audit verifiers. |
| `SUPABASE_ACCESS_TOKEN` | GitHub Actions secret | Short-lived management API token for metrics. |
| `SUPABASE_DISPATCHER_SIGNING_KEY` | Supabase Secrets (`scheduled-dispatcher`) | HMAC key used to sign message payloads. |
| `SUPABASE_BACKUP_TOKEN` | Azure Automation credential store | Token to request backup metadata. |

## 3. Monitoring Dashboard Integration

- Metrics Source: `SupabaseMonitoringService` (see `src/NexaCRM.Service/Admin.Core/Services/SupabaseMonitoringService.cs`).
- Export Path: OTLP traces forwarded to Grafana Cloud dataset `nexa.supabase`.
- Key Widgets:
  - Database query latency (p50/p95) with SLO target &le; 200ms.
  - Connection pool usage (max threshold 80%).
  - Error rate from PostgREST & Edge Functions.

## 4. Audit & Integration Event Verification

- `SupabaseAuditSyncVerifier` validates that `audit_logs` and `integration_events` remain consistent within a 5 event drift during the past 2 hours.
- Results are surfaced through the Admin portal (`/admin/ops`) and feed the weekly compliance report.

## 5. Escalation Matrix

| Severity | Primary Contact | Secondary |
| --- | --- | --- |
| Sev 1 (Data loss, outage) | Platform Engineering on-call | CTO |
| Sev 2 (Degraded throughput) | Observability on-call | Messaging Team lead |
| Sev 3 (Non-blocking alerts) | Ops duty engineer | Product Manager |

## 6. References

- `supabase/BACKUP_AND_RECOVERY.md`
- `supabase/functions/scheduled-dispatcher/README.md`
- `supabase/SUPABASE_INTEGRATION_GUIDE.md`


# Supabase Backup & Point-in-Time Recovery Runbook

This runbook documents the operational steps required to protect the NexaCRM Supabase project with automated backups, point-in-time recovery (PITR), and recurring validation.

## 1. Backup Strategy Overview

| Item | Value |
| --- | --- |
| Backup Scope | `crm_core`, `realtime`, `storage` databases and `storage` buckets |
| Retention | 30 days PITR, 7 daily snapshots, 12 monthly archives |
| Storage Location | Supabase managed backups (primary) + Azure Blob archive (secondary) |
| Responsible Team | NexaCRM Platform Engineering |

### 1.1 Automated Backups

1. Enable **Point-in-Time Recovery (PITR)** for the primary Supabase database with a 30-day window.
2. Configure scheduled **daily base backups** at 01:00 UTC via the Supabase Dashboard (`Settings → Backups`).
3. Mirror the latest daily backup to Azure Blob Storage using the Supabase management API (`/v1/projects/{ref}/backups/latest`) and the `nexa-ops` automation account.

### 1.2 Storage Bucket Snapshots

1. Execute `supabase storage list` nightly to enumerate buckets.
2. For each bucket, trigger `supabase storage download` into the temporary staging path `/tmp/supabase-storage/{bucket}`.
3. Upload the staged data to Azure Blob Storage with immutable retention policies (7 days).

## 2. Recovery Procedures

### 2.1 Point-in-Time Recovery (Database)

1. Identify the incident timestamp and calculate the nearest PITR target (`incident_time - 5 minutes`).
2. Use the Supabase dashboard or management API to create a **new recovery branch** at the target timestamp.
3. Validate schema consistency by running `supabase db diff --linked` against the recovered branch.
4. Redirect staging traffic to the recovered branch and execute smoke tests.
5. Promote the recovery branch to production or replay data into the primary project depending on impact analysis.

### 2.2 Storage Restoration

1. Locate the archived bucket snapshot in Azure Blob Storage using the incident date.
2. Restore the files to a temporary bucket (`restored-{bucket}-{yyyyMMdd}`) using the Supabase Storage admin API.
3. Validate object ACLs and signed URLs.
4. Swap the restored bucket into production after stakeholder approval.

## 3. Verification Cadence

| Frequency | Task | Owner |
| --- | --- | --- |
| Weekly | Restore PITR branch in staging and run integration smoke tests. | Platform Eng. |
| Monthly | Perform full storage restore rehearsal in staging. | Platform Eng. |
| Quarterly | Audit backup retention in Supabase and Azure Blob; rotate API keys. | Security Ops |

## 4. Tooling & Automation

- GitHub Actions workflow `ops-supabase-backup.yml` (to be added) calls the Supabase Management API with the `SUPABASE_BACKUP_TOKEN` secret to verify backup freshness.
- Azure Automation runbook `Supabase-PITR-Drill` orchestrates quarterly recovery rehearsals and posts summaries to the #ops channel via Teams webhook.
- All automation accounts are documented in `docs/operations/SUPABASE_OPERATIONS.md`.

## 5. Incident Checklists

1. Freeze write operations by disabling public connections or revoking service-role keys.
2. Capture timestamps, impacted services, and user reports in the incident tracker.
3. Follow Section 2 for recovery and document all actions.
4. After resolution, review monitoring alerts and update postmortem with findings.

## 6. References

- [Supabase PITR Documentation](https://supabase.com/docs/guides/platform/backups)
- [Azure Blob Immutable Storage](https://learn.microsoft.com/azure/storage/blobs/immutable-storage-overview)
- `docs/operations/SUPABASE_OPERATIONS.md` for automation ownership.


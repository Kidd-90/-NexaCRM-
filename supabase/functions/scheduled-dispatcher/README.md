# Supabase Edge Function — `scheduled-dispatcher`

This function replaces the legacy worker that handled scheduled SMS and email dispatching. It is invoked every minute through Supabase cron triggers.

## Responsibilities

1. Select up to 50 pending rows from `scheduled_dispatches` where `scheduled_at <= now()`.
2. Mark the batch as `processing` to prevent duplicate delivery.
3. Post each job to the appropriate outbound webhook (SMS or email) with an HMAC signature.
4. Update each row as `sent` or `failed` with diagnostic information.

## Deployment

```bash
supabase functions deploy scheduled-dispatcher \
  --project-ref $SUPABASE_PROJECT_REF \
  --no-verify-jwt
```

Required secrets:

```bash
supabase secrets set \
  SUPABASE_SERVICE_ROLE_KEY=$SERVICE_ROLE_KEY \
  SUPABASE_DISPATCHER_SIGNING_KEY=$DISPATCH_SIGNING_KEY \
  SMS_DISPATCH_WEBHOOK=$SMS_WEBHOOK \
  EMAIL_DISPATCH_WEBHOOK=$EMAIL_WEBHOOK
```

## Cron Schedule

The `cron.yaml` file configures the function to run once per minute:

```yaml
version: 1
functions:
  - name: scheduled-dispatcher
    schedule: "* * * * *"
```

Apply the schedule with:

```bash
supabase functions deploy scheduled-dispatcher --project-ref $SUPABASE_PROJECT_REF --no-verify-jwt
supabase functions deploy scheduled-dispatcher --project-ref $SUPABASE_PROJECT_REF --no-verify-jwt --schedule cron.yaml
```

## Observability

- Logs stream to Supabase Logflare; filter by `scheduled-dispatcher`.
- Metrics (success/failure counts) are forwarded via `SupabaseMonitoringService` to the central dashboard.
- Alerts trigger when failure ratio &gt; 10% over 15 minutes.


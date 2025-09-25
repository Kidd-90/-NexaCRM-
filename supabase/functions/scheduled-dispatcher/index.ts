// deno-lint-ignore-file no-explicit-any
import "https://deno.land/std@0.224.0/dotenv/load.ts";
import { createClient } from "https://esm.sh/@supabase/supabase-js@2";

const SUPABASE_URL = Deno.env.get("SUPABASE_URL");
const SERVICE_ROLE_KEY = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
const DISPATCH_SIGNING_KEY = Deno.env.get("SUPABASE_DISPATCHER_SIGNING_KEY");

if (!SUPABASE_URL || !SERVICE_ROLE_KEY || !DISPATCH_SIGNING_KEY) {
  throw new Error("Missing Supabase dispatcher environment variables.");
}

const supabase = createClient(SUPABASE_URL, SERVICE_ROLE_KEY, {
  auth: { persistSession: false },
});

interface ScheduledJob {
  id: string;
  channel: "sms" | "email";
  payload: Record<string, any>;
  scheduled_at: string;
  status: "pending" | "processing" | "sent" | "failed";
}

const MAX_BATCH = 50;

async function fetchPendingJobs(): Promise<ScheduledJob[]> {
  const nowIso = new Date().toISOString();
  const { data, error } = await supabase
    .from<ScheduledJob>("scheduled_dispatches")
    .select("id, channel, payload, scheduled_at, status")
    .lte("scheduled_at", nowIso)
    .eq("status", "pending")
    .order("scheduled_at", { ascending: true })
    .limit(MAX_BATCH);

  if (error) {
    console.error("Failed to load scheduled jobs", error);
    throw error;
  }

  return data ?? [];
}

async function markProcessing(ids: string[]): Promise<void> {
  if (ids.length === 0) {
    return;
  }

  const { error } = await supabase
    .from("scheduled_dispatches")
    .update({ status: "processing" })
    .in("id", ids);

  if (error) {
    console.error("Failed to mark jobs processing", error);
    throw error;
  }
}

async function finalizeJob(id: string, status: "sent" | "failed", errorMessage?: string) {
  const update: Record<string, any> = { status, processed_at: new Date().toISOString() };
  if (status === "failed") {
    update.failure_reason = errorMessage ?? "Unknown error";
  }

  const { error } = await supabase
    .from("scheduled_dispatches")
    .update(update)
    .eq("id", id);

  if (error) {
    console.error(`Failed to finalize job ${id}`, error);
  }
}

async function signPayload(payload: Record<string, any>): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(JSON.stringify(payload));
  const keyData = encoder.encode(DISPATCH_SIGNING_KEY);
  const cryptoKey = await crypto.subtle.importKey(
    "raw",
    keyData,
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign"],
  );

  const signature = await crypto.subtle.sign("HMAC", cryptoKey, data);
  const bytes = new Uint8Array(signature);
  return btoa(String.fromCharCode(...bytes));
}

async function invokeOutboundDispatcher(job: ScheduledJob): Promise<void> {
  const signedPayload = await signPayload(job.payload);
  const endpoint = job.channel === "sms"
    ? Deno.env.get("SMS_DISPATCH_WEBHOOK")
    : Deno.env.get("EMAIL_DISPATCH_WEBHOOK");

  if (!endpoint) {
    throw new Error(`Missing webhook endpoint for channel ${job.channel}`);
  }

  const response = await fetch(endpoint, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-NexaCRM-Signature": signedPayload,
    },
    body: JSON.stringify({
      id: job.id,
      channel: job.channel,
      payload: job.payload,
      scheduled_at: job.scheduled_at,
    }),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Dispatcher responded ${response.status}: ${errorText}`);
  }
}

Deno.serve(async () => {
  try {
    const jobs = await fetchPendingJobs();
    if (jobs.length === 0) {
      return new Response(JSON.stringify({ processed: 0 }), {
        headers: { "Content-Type": "application/json" },
      });
    }

    await markProcessing(jobs.map((job) => job.id));

    let successes = 0;
    let failures = 0;
    for (const job of jobs) {
      try {
        await invokeOutboundDispatcher(job);
        await finalizeJob(job.id, "sent");
        successes += 1;
      } catch (dispatchError) {
        console.error(`Dispatch failure for job ${job.id}`, dispatchError);
        await finalizeJob(job.id, "failed", dispatchError instanceof Error ? dispatchError.message : "Unknown error");
        failures += 1;
      }
    }

    return new Response(JSON.stringify({ processed: jobs.length, successes, failures }), {
      headers: { "Content-Type": "application/json" },
    });
  } catch (error) {
    console.error("Scheduled dispatcher execution failed", error);
    return new Response(JSON.stringify({ error: error instanceof Error ? error.message : "Unknown" }), {
      status: 500,
      headers: { "Content-Type": "application/json" },
    });
  }
});


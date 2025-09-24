using Supabase.Gotrue;

namespace NexaCRM.WebClient.Services.Supabase;

public sealed record SupabaseSignInResult(bool Succeeded, Session? Session, string? ErrorMessage);

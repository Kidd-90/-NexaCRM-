using System.Diagnostics.CodeAnalysis;

namespace NexaCRM.Service.Supabase.Configuration;

/// <summary>
/// Provides default values that allow the application to run in an offline mode when Supabase configuration is absent.
/// </summary>
public static class SupabaseClientDefaults
{
    public const string OfflineUrl = "https://localhost";

    [StringSyntax(StringSyntaxAttribute.GuidFormat, "N")]
    public const string OfflineAnonKey =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJvZmZsaW5lIiwicm9sZSI6ImFub24iLCJpYXQiOjAsImV4cCI6MjUzNDAyMzAwNzk5fQ.b2ZmbGluZS1zaWduYXR1cmU";
}

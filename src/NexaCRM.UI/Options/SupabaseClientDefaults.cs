using System.Diagnostics.CodeAnalysis;

namespace NexaCRM.UI.Options;

/// <summary>
/// Provides default values that allow the application to run in an offline mode when Supabase configuration is absent.
/// </summary>
public static class SupabaseClientDefaults
{
    public const string OfflineUrl = "https://localhost";

    [StringSyntax(StringSyntaxAttribute.GuidFormat, "N")]
    public const string OfflineAnonKey = "00000000000000000000000000000000";
}

using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Represents the configuration values required to connect NexaCRM services to Supabase.
/// </summary>
public sealed record SupabaseSettings
{
    /// <summary>
    /// The configuration section key used by default for binding.
    /// </summary>
    public const string SectionName = "Supabase";

    /// <summary>
    /// Base URL of the Supabase project.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Anon key that is safe to expose to browser applications.
    /// </summary>
    public string? AnonKey { get; set; }

    /// <summary>
    /// Service role key used by trusted server-side workloads.
    /// </summary>
    public string? ServiceRoleKey { get; set; }

    /// <summary>
    /// Database-specific connection options.
    /// </summary>
    [Required]
    public SupabaseDatabaseSettings Database { get; set; } = new();

    /// <summary>
    /// Client behaviour flags that control realtime and auth behaviour.
    /// </summary>
    [Required]
    public SupabaseClientOptions Client { get; set; } = new();
}

/// <summary>
/// Holds the database connection string that will be used when a service connects directly via Npgsql.
/// </summary>
public sealed record SupabaseDatabaseSettings
{
    /// <summary>
    /// Connection string pointing to the Supabase managed PostgreSQL instance.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Mirrors the core <see cref="global::Supabase.SupabaseOptions"/> values we care about and allows them to be configured via options.
/// </summary>
public sealed record SupabaseClientOptions
{
    /// <summary>
    /// When true the SDK will automatically attempt to connect to the realtime websocket.
    /// </summary>
    public bool AutoConnectRealtime { get; set; } = false;

    /// <summary>
    /// When true the SDK refreshes tokens automatically before they expire.
    /// </summary>
    public bool AutoRefreshToken { get; set; } = true;
}

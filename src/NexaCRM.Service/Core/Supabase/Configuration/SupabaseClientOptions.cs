using System.ComponentModel.DataAnnotations;

namespace NexaCRM.Service.Supabase.Configuration;

/// <summary>
/// Configuration required to initialize the Supabase client inside the Blazor WebAssembly application.
/// </summary>
public sealed class SupabaseClientOptions
{
    public const string SectionName = "Supabase";

    [Required]
    [Url]
    public string? Url { get; set; }

    [Required]
    public string? AnonKey { get; set; }

    public string? ServiceKey { get; set; }
}

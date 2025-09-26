using System.ComponentModel.DataAnnotations;

namespace NexaCRM.WebClient.Options;

/// <summary>
/// Configuration required to initialize the Supabase client inside the Blazor WebAssembly application.
/// </summary>
public sealed class SupabaseClientOptions
{
    public const string SectionName = "Supabase";

    [Required]
    [Url]
    public string? Url { get; init; }

    [Required]
    public string? AnonKey { get; init; }
}

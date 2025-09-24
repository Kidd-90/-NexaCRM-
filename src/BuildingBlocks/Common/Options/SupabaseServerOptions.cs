using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Common.Options;

/// <summary>
/// Strongly typed representation of the Supabase configuration used by server side services.
/// </summary>
public sealed class SupabaseServerOptions
{
    public const string SectionName = "Supabase";

    /// <summary>
    /// Base URL for the Supabase project (e.g. https://xyzcompany.supabase.co).
    /// </summary>
    [Required]
    [Url]
    public string? Url { get; init; }

    /// <summary>
    /// Service role key used by privileged server-side components.
    /// </summary>
    [Required]
    public string? ServiceRoleKey { get; init; }

    /// <summary>
    /// JWT secret used to validate tokens issued by Supabase.
    /// </summary>
    [Required]
    public string? JwtSecret { get; init; }

    /// <summary>
    /// Optional anon key for scenarios where the server relays client calls.
    /// </summary>
    public string? AnonKey { get; init; }

    /// <summary>
    /// Binds the <see cref="SupabaseServerOptions"/> from configuration and enforces validation.
    /// </summary>
    public static SupabaseServerOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration
            .GetSection(SectionName)
            .Get<SupabaseServerOptions>() ?? new SupabaseServerOptions();

        options.Validate();
        return options;
    }

    /// <summary>
    /// Validates the option values using <see cref="DataAnnotations"/> attributes.
    /// </summary>
    public void Validate()
    {
        Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
    }
}

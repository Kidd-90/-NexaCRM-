using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Common.Options;

/// <summary>
/// Configuration that controls Supabase audit & integration event drift validation.
/// </summary>
public sealed class SupabaseAuditSyncOptions
{
    public const string SectionName = "SupabaseAuditSync";

    /// <summary>
    /// Maximum tolerated difference between audit and integration event counts within the validation window.
    /// </summary>
    [Range(0, 1000)]
    public int AllowedDriftCount { get; init; } = 5;

    /// <summary>
    /// Time window to evaluate for drift.
    /// </summary>
    [Range(5, 1440)]
    public int WindowMinutes { get; init; } = 120;

    public static SupabaseAuditSyncOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var options = configuration.GetSection(SectionName).Get<SupabaseAuditSyncOptions>() ?? new SupabaseAuditSyncOptions();
        options.Validate();
        return options;
    }

    public void Validate()
    {
        Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
    }
}


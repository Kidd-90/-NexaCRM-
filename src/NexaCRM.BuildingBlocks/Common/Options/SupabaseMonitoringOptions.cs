using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Common.Options;

/// <summary>
/// Configuration required to query the Supabase management metrics API.
/// </summary>
public sealed class SupabaseMonitoringOptions
{
    public const string SectionName = "SupabaseMonitoring";

    private static readonly string[] DefaultMetricKeys =
    [
        "db_connections",
        "db_cpu_seconds",
        "db_tps",
        "rest_request_errors"
    ];

    /// <summary>
    /// Base URL for the Supabase management API.
    /// </summary>
    [Required]
    [Url]
    public string ApiBaseUrl { get; init; } = "https://api.supabase.com/v1/projects/";

    /// <summary>
    /// Supabase project reference (e.g. abcd1234).
    /// </summary>
    [Required]
    [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Supabase project ref must be lowercase alphanumeric.")]
    public string? ProjectRef { get; init; }

    /// <summary>
    /// Access token used to authorize against the management API.
    /// </summary>
    [Required]
    public string? AccessToken { get; init; }

    /// <summary>
    /// Window, in minutes, to request metrics for.
    /// </summary>
    [Range(5, 1440)]
    public int WindowMinutes { get; init; } = 60;

    /// <summary>
    /// Metric keys to retrieve from the API.
    /// </summary>
    [MinLength(1)]
    public string[] MetricKeys { get; init; } = DefaultMetricKeys;

    public static SupabaseMonitoringOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var options = configuration.GetSection(SectionName).Get<SupabaseMonitoringOptions>() ?? new SupabaseMonitoringOptions();
        options.Validate();
        return options;
    }

    public void Validate()
    {
        Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
    }
}


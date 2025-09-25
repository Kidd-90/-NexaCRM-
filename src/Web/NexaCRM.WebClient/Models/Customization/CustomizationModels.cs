using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Customization;

/// <summary>
/// Settings applied to all users in a specific organization.
/// </summary>
public sealed record OrganizationSettings
{
    public Guid OrganizationId { get; init; }

    public string TimeZone { get; init; } = "UTC";

    public string Locale { get; init; } = "en-US";

    public bool EnableAuditTrail { get; init; }

    public IReadOnlyDictionary<string, bool> FeatureFlags { get; init; } =
        new Dictionary<string, bool>();
}

/// <summary>
/// User level personalization captured from the settings panel.
/// </summary>
public sealed record UserPreferences
{
    public Guid UserId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Theme { get; init; } = "light";

    public IReadOnlyDictionary<string, string> Preferences { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Captures the layout of dashboard widgets for a user.
/// </summary>
public sealed record DashboardWidget
{
    public Guid WidgetId { get; init; }

    public Guid UserId { get; init; }

    public string WidgetType { get; init; } = string.Empty;

    public int Order { get; init; }

    public IReadOnlyDictionary<string, string> Settings { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Records a KPI snapshot for trend analysis dashboards.
/// </summary>
public sealed record KpiSnapshot
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public string KpiName { get; init; } = string.Empty;

    public decimal Value { get; init; }

    public DateTime CapturedAtUtc { get; init; }
}

using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Customization;

public sealed class OrganizationSettings
{
    public Guid OrganizationId { get; init; }

    public string Locale { get; init; } = "ko-KR";

    public string Timezone { get; init; } = "Asia/Seoul";

    public string Theme { get; init; } = "light";

    public IReadOnlyDictionary<string, bool> FeatureFlags { get; init; }
        = new Dictionary<string, bool>();
}

public sealed class UserPreferences
{
    public Guid UserId { get; init; }

    public string Theme { get; init; } = "system";

    public string DateFormat { get; init; } = "yyyy-MM-dd";

    public bool EnableNotifications { get; init; } = true;

    public IReadOnlyDictionary<string, string> WidgetPreferences { get; init; }
        = new Dictionary<string, string>();
}

public sealed class DashboardWidget
{
    public Guid WidgetId { get; init; }

    public string Type { get; init; } = string.Empty;

    public int Order { get; init; }

    public IReadOnlyDictionary<string, string> Configuration { get; init; }
        = new Dictionary<string, string>();
}

public sealed class DashboardLayout
{
    public Guid UserId { get; init; }

    public IReadOnlyList<DashboardWidget> Widgets { get; init; } = Array.Empty<DashboardWidget>();
}

public sealed class KpiSnapshot
{
    public Guid Id { get; init; }

    public string Metric { get; init; } = string.Empty;

    public decimal Value { get; init; }

    public DateTime CapturedAt { get; init; }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Customization;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock;

public sealed class MockSettingsCustomizationService : ISettingsCustomizationService
{
    private readonly ConcurrentDictionary<Guid, OrganizationSettings> _organizationSettings = new();
    private readonly ConcurrentDictionary<Guid, UserPreferences> _userPreferences = new();
    private readonly ConcurrentDictionary<Guid, List<DashboardWidget>> _dashboardLayouts = new();
    private readonly ConcurrentDictionary<Guid, List<KpiSnapshot>> _kpiSnapshots = new();

    public Task<OrganizationSettings> GetOrganizationSettingsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var settings = _organizationSettings.GetOrAdd(organizationId, id => new OrganizationSettings
        {
            OrganizationId = id,
            Locale = "ko-KR",
            TimeZone = "Asia/Seoul",
            EnableAuditTrail = true,
            FeatureFlags = new Dictionary<string, bool>
            {
                ["advancedReporting"] = true,
                ["autoAssignment"] = false
            }
        });

        return Task.FromResult(settings);
    }

    public Task SaveOrganizationSettingsAsync(
        OrganizationSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _organizationSettings[settings.OrganizationId] = settings;
        return Task.CompletedTask;
    }

    public Task<UserPreferences> GetUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var preferences = _userPreferences.GetOrAdd(userId, id => new UserPreferences
        {
            UserId = id,
            OrganizationId = Guid.Empty,
            Theme = "light",
            Preferences = new Dictionary<string, string>
            {
                ["nav.compact"] = "false"
            }
        });

        return Task.FromResult(preferences);
    }

    public Task SaveUserPreferencesAsync(
        UserPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        if (preferences is null)
        {
            throw new ArgumentNullException(nameof(preferences));
        }

        _userPreferences[preferences.UserId] = preferences;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DashboardWidget>> GetDashboardLayoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var layout = _dashboardLayouts.GetOrAdd(userId, _ => new List<DashboardWidget>
        {
            new()
            {
                WidgetId = Guid.NewGuid(),
                UserId = userId,
                WidgetType = "sales-performance",
                Order = 0,
                Settings = new Dictionary<string, string>
                {
                    ["period"] = "month"
                }
            },
            new()
            {
                WidgetId = Guid.NewGuid(),
                UserId = userId,
                WidgetType = "activity-stream",
                Order = 1,
                Settings = new Dictionary<string, string>
                {
                    ["showCompleted"] = "true"
                }
            }
        });

        return Task.FromResult<IReadOnlyList<DashboardWidget>>(layout.OrderBy(widget => widget.Order).ToList());
    }

    public Task SaveDashboardLayoutAsync(
        Guid userId,
        IEnumerable<DashboardWidget> widgets,
        CancellationToken cancellationToken = default)
    {
        if (widgets is null)
        {
            throw new ArgumentNullException(nameof(widgets));
        }

        _dashboardLayouts[userId] = widgets.Select(widget => widget with { UserId = userId }).ToList();
        return Task.CompletedTask;
    }

    public Task RecordKpiSnapshotAsync(
        KpiSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        var list = _kpiSnapshots.GetOrAdd(snapshot.OrganizationId, _ => new List<KpiSnapshot>());
        list.Add(snapshot with { CapturedAtUtc = snapshot.CapturedAtUtc == default ? DateTime.UtcNow : snapshot.CapturedAtUtc });
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KpiSnapshot>> GetKpiHistoryAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var history = _kpiSnapshots.TryGetValue(organizationId, out var list)
            ? list.OrderByDescending(item => item.CapturedAtUtc).Take(50).ToList()
            : new List<KpiSnapshot>();

        return Task.FromResult<IReadOnlyList<KpiSnapshot>>(history);
    }
}

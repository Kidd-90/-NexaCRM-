using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.Customization;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.Service.Supabase;

namespace NexaCRM.WebClient.Services.SupabaseEnterprise;

/// <summary>
/// Stores organization and user personalization metadata in a Supabase backed cache.
/// </summary>
public sealed class SupabaseSettingsCustomizationService : ISettingsCustomizationService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly SupabaseEnterpriseDataStore _store;
    private readonly ILogger<SupabaseSettingsCustomizationService> _logger;

    public SupabaseSettingsCustomizationService(
        SupabaseClientProvider clientProvider,
        SupabaseEnterpriseDataStore store,
        ILogger<SupabaseSettingsCustomizationService> logger)
    {
        _clientProvider = clientProvider;
        _store = store;
        _logger = logger;
    }

    public async Task<OrganizationSettings> GetOrganizationSettingsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        if (_store.OrganizationSettings.TryGetValue(organizationId, out var settings))
        {
            return settings;
        }

        var defaults = new OrganizationSettings
        {
            OrganizationId = organizationId,
            FeatureFlags = new Dictionary<string, bool>
            {
                ["files.enabled"] = true,
                ["communications.enabled"] = true
            }
        };

        _store.OrganizationSettings.TryAdd(organizationId, defaults);
        return defaults;
    }

    public Task SaveOrganizationSettingsAsync(
        OrganizationSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(settings.OrganizationId));
        }

        _store.OrganizationSettings[settings.OrganizationId] = settings;
        return Task.CompletedTask;
    }

    public async Task<UserPreferences> GetUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        if (_store.UserPreferences.TryGetValue(userId, out var preferences))
        {
            return preferences;
        }

        var defaultPreferences = new UserPreferences
        {
            UserId = userId,
            OrganizationId = Guid.Empty,
            Theme = "light"
        };

        _store.UserPreferences.TryAdd(userId, defaultPreferences);
        return defaultPreferences;
    }

    public Task SaveUserPreferencesAsync(
        UserPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        if (preferences.UserId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(preferences.UserId));
        }

        _store.UserPreferences[preferences.UserId] = preferences;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DashboardWidget>> GetDashboardLayoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        var widgets = _store.DashboardLayouts.TryGetValue(userId, out var layout)
            ? layout.OrderBy(w => w.Order).ToList()
            : new List<DashboardWidget>();

        return Task.FromResult<IReadOnlyList<DashboardWidget>>(widgets);
    }

    public Task SaveDashboardLayoutAsync(
        Guid userId,
        IEnumerable<DashboardWidget> widgets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(widgets);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        var ordered = widgets
            .Select((widget, index) => widget with { Order = index })
            .ToList();

        _store.DashboardLayouts[userId] = ordered;
        return Task.CompletedTask;
    }

    public Task RecordKpiSnapshotAsync(
        KpiSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(snapshot.OrganizationId));
        }

        var snapshots = _store.KpiSnapshots.GetOrAdd(snapshot.OrganizationId, _ => new List<KpiSnapshot>());
        snapshots.Add(snapshot);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KpiSnapshot>> GetKpiHistoryAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        var snapshots = _store.KpiSnapshots.TryGetValue(organizationId, out var list)
            ? list.OrderByDescending(s => s.CapturedAtUtc).ToList()
            : new List<KpiSnapshot>();

        return Task.FromResult<IReadOnlyList<KpiSnapshot>>(snapshots);
    }

    private async Task EnsureClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _clientProvider.GetClientAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase client unavailable; operating against in-memory store.");
        }
    }
}

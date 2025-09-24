using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexaCRM.WebClient.Models.Customization;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseSettingsCustomizationService : ISettingsCustomizationService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseSettingsCustomizationService> _logger;

    public SupabaseSettingsCustomizationService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseSettingsCustomizationService> logger)
    {
        _clientProvider = clientProvider;
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

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<OrganizationSettingsRecord>()
                .Filter(x => x.OrganizationId, PostgrestOperator.Equals, organizationId)
                .Single(cancellationToken: cancellationToken);

            if (response.Model is null)
            {
                return new OrganizationSettings { OrganizationId = organizationId };
            }

            var featureFlags = string.IsNullOrWhiteSpace(response.Model.FeatureFlagsJson)
                ? new Dictionary<string, bool>()
                : JsonConvert.DeserializeObject<Dictionary<string, bool>>(response.Model.FeatureFlagsJson!)
                    ?? new Dictionary<string, bool>();

            return new OrganizationSettings
            {
                OrganizationId = response.Model.OrganizationId,
                Locale = response.Model.Locale,
                Timezone = response.Model.Timezone,
                Theme = response.Model.Theme,
                FeatureFlags = featureFlags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load organization settings from Supabase for {OrgId}.", organizationId);
            throw;
        }
    }

    public async Task SaveOrganizationSettingsAsync(
        OrganizationSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(settings));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var existing = await client.From<OrganizationSettingsRecord>()
                .Filter(x => x.OrganizationId, PostgrestOperator.Equals, settings.OrganizationId)
                .Single(cancellationToken: cancellationToken);

            var record = new OrganizationSettingsRecord
            {
                Id = existing.Model?.Id ?? Guid.NewGuid(),
                OrganizationId = settings.OrganizationId,
                Locale = settings.Locale,
                Timezone = settings.Timezone,
                Theme = settings.Theme,
                FeatureFlagsJson = JsonConvert.SerializeObject(settings.FeatureFlags)
            };

            if (existing.Model is null)
            {
                await client.From<OrganizationSettingsRecord>().Insert(record, cancellationToken: cancellationToken);
            }
            else
            {
                await client.From<OrganizationSettingsRecord>().Update(record);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist organization settings for {OrgId}.", settings.OrganizationId);
            throw;
        }
    }

    public async Task<UserPreferences> GetUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<UserPreferenceRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Single(cancellationToken: cancellationToken);

            if (response.Model is null)
            {
                return new UserPreferences { UserId = userId };
            }

            var widgetPreferences = string.IsNullOrWhiteSpace(response.Model.WidgetPreferencesJson)
                ? new Dictionary<string, string>()
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Model.WidgetPreferencesJson!)
                    ?? new Dictionary<string, string>();

            return new UserPreferences
            {
                UserId = response.Model.UserId,
                Theme = response.Model.Theme,
                DateFormat = response.Model.DateFormat,
                EnableNotifications = response.Model.EnableNotifications,
                WidgetPreferences = widgetPreferences
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user preferences from Supabase for {UserId}.", userId);
            throw;
        }
    }

    public async Task SaveUserPreferencesAsync(
        UserPreferences preferences,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        if (preferences.UserId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(preferences));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var existing = await client.From<UserPreferenceRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, preferences.UserId)
                .Single(cancellationToken: cancellationToken);

            var record = new UserPreferenceRecord
            {
                Id = existing.Model?.Id ?? Guid.NewGuid(),
                UserId = preferences.UserId,
                Theme = preferences.Theme,
                DateFormat = preferences.DateFormat,
                EnableNotifications = preferences.EnableNotifications,
                WidgetPreferencesJson = JsonConvert.SerializeObject(preferences.WidgetPreferences)
            };

            if (existing.Model is null)
            {
                await client.From<UserPreferenceRecord>().Insert(record, cancellationToken: cancellationToken);
            }
            else
            {
                await client.From<UserPreferenceRecord>().Update(record);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user preferences for {UserId}.", preferences.UserId);
            throw;
        }
    }

    public async Task<DashboardLayout> GetDashboardLayoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<DashboardWidgetRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Order(x => x.DisplayOrder, PostgrestOrdering.Ascending)
                .Get(cancellationToken: cancellationToken);

            var widgets = response.Models
                .Select(record => new DashboardWidget
                {
                    WidgetId = record.Id,
                    Type = record.WidgetType,
                    Order = record.DisplayOrder,
                    Configuration = DeserializeConfiguration(record.ConfigurationJson)
                })
                .ToList();

            return new DashboardLayout
            {
                UserId = userId,
                Widgets = widgets
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard layout for {UserId}.", userId);
            throw;
        }
    }

    public async Task SaveDashboardLayoutAsync(
        DashboardLayout layout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(layout);
        if (layout.UserId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(layout));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();

            await client.From<DashboardWidgetRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, layout.UserId)
                .Delete(cancellationToken: cancellationToken);

            var records = layout.Widgets.Select(widget => new DashboardWidgetRecord
            {
                Id = widget.WidgetId == Guid.Empty ? Guid.NewGuid() : widget.WidgetId,
                UserId = layout.UserId,
                WidgetType = widget.Type,
                DisplayOrder = widget.Order,
                ConfigurationJson = JsonConvert.SerializeObject(widget.Configuration)
            });

            if (records.Any())
            {
                await client.From<DashboardWidgetRecord>().Insert(records, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store dashboard layout for {UserId}.", layout.UserId);
            throw;
        }
    }

    public async Task<IReadOnlyList<KpiSnapshot>> GetKpiSnapshotsAsync(
        string metric,
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<KpiSnapshotRecord>()
                .Filter(x => x.Metric, PostgrestOperator.Equals, metric)
                .Filter(x => x.CapturedAt, PostgrestOperator.GreaterThanOrEqual, since)
                .Order(x => x.CapturedAt, PostgrestOrdering.Ascending)
                .Get(cancellationToken: cancellationToken);

            return response.Models
                .Select(record => new KpiSnapshot
                {
                    Id = record.Id,
                    Metric = record.Metric,
                    Value = record.Value,
                    CapturedAt = record.CapturedAt
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load KPI snapshots for metric {Metric}.", metric);
            throw;
        }
    }

    private static IReadOnlyDictionary<string, string> DeserializeConfiguration(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>();
        }

        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json!)
            ?? new Dictionary<string, string>();
    }
}

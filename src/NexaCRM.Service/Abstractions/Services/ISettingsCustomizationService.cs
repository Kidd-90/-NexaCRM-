using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Customization;

namespace NexaCRM.UI.Services.Interfaces;

/// <summary>
/// Handles personalization data such as organization defaults and user dashboard layouts.
/// </summary>
public interface ISettingsCustomizationService
{
    Task<OrganizationSettings> GetOrganizationSettingsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task SaveOrganizationSettingsAsync(
        OrganizationSettings settings,
        CancellationToken cancellationToken = default);

    Task<UserPreferences> GetUserPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveUserPreferencesAsync(
        UserPreferences preferences,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DashboardWidget>> GetDashboardLayoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveDashboardLayoutAsync(
        Guid userId,
        IEnumerable<DashboardWidget> widgets,
        CancellationToken cancellationToken = default);

    Task RecordKpiSnapshotAsync(
        KpiSnapshot snapshot,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KpiSnapshot>> GetKpiHistoryAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}

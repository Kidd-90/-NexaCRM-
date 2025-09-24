using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Customization;

namespace NexaCRM.WebClient.Services.Interfaces;

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

    Task<DashboardLayout> GetDashboardLayoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task SaveDashboardLayoutAsync(
        DashboardLayout layout,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KpiSnapshot>> GetKpiSnapshotsAsync(
        string metric,
        DateTime since,
        CancellationToken cancellationToken = default);
}

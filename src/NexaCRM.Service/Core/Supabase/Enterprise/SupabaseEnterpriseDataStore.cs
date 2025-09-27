using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NexaCRM.UI.Models.Customization;
using NexaCRM.UI.Models.FileHub;
using NexaCRM.UI.Models.Governance;
using NexaCRM.UI.Models.Sync;

namespace NexaCRM.Service.Supabase.Enterprise;

/// <summary>
/// Thread-safe in-memory data store that simulates Supabase persistence for offline development scenarios.
/// </summary>
public sealed class SupabaseEnterpriseDataStore
{
    public ConcurrentDictionary<Guid, UserAccount> Users { get; } = new();

    public ConcurrentDictionary<Guid, HashSet<string>> UserRoles { get; } = new();

    public ConcurrentDictionary<Guid, List<SecurityAuditLogEntry>> AuditLogs { get; } = new();

    public ConcurrentDictionary<Guid, SecurityPolicy> SecurityPolicies { get; } = new();

    public ConcurrentDictionary<Guid, PasswordResetTicket> PasswordResetTickets { get; } = new();

    public ConcurrentDictionary<Guid, OrganizationSettings> OrganizationSettings { get; } = new();

    public ConcurrentDictionary<Guid, UserPreferences> UserPreferences { get; } = new();

    public ConcurrentDictionary<Guid, List<DashboardWidget>> DashboardLayouts { get; } = new();

    public ConcurrentDictionary<Guid, List<KpiSnapshot>> KpiSnapshots { get; } = new();

    public ConcurrentDictionary<Guid, FileDocument> Documents { get; } = new();

    public ConcurrentDictionary<Guid, List<FileVersion>> DocumentVersions { get; } = new();

    public ConcurrentDictionary<Guid, CommunicationThread> Threads { get; } = new();

    public ConcurrentDictionary<Guid, List<ThreadMessage>> ThreadMessages { get; } = new();

    public ConcurrentDictionary<Guid, List<Guid>> DocumentThreads { get; } = new();

    public ConcurrentDictionary<Guid, SyncEnvelope> SyncEnvelopes { get; } = new();

    public ConcurrentDictionary<Guid, List<SyncConflict>> SyncConflicts { get; } = new();
}

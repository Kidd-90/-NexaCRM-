using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Sync;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ISyncOrchestrationService
{
    Task<SyncPlan> BuildSyncPlanAsync(
        Guid userId,
        SyncPolicy policy,
        CancellationToken cancellationToken = default);

    Task RecordClientEnvelopeAsync(
        SyncEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncConflict>> GetConflictsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ResolveConflictsAsync(
        IReadOnlyCollection<SyncConflict> conflicts,
        CancellationToken cancellationToken = default);
}

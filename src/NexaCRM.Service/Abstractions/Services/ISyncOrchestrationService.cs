using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Sync;

namespace NexaCRM.UI.Services.Interfaces;

/// <summary>
/// Coordinates offline envelopes and conflict tracking.
/// </summary>
public interface ISyncOrchestrationService
{
    Task<SyncEnvelope> RegisterEnvelopeAsync(
        Guid envelopeId,
        Guid organizationId,
        Guid userId,
        IEnumerable<SyncItem> items,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncEnvelope>> GetPendingEnvelopesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task MarkEnvelopeAsAppliedAsync(
        Guid envelopeId,
        DateTime appliedAtUtc,
        CancellationToken cancellationToken = default);

    Task<SyncConflict> RegisterConflictAsync(
        SyncConflict conflict,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncConflict>> GetConflictsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Sync;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock;

public sealed class MockSyncOrchestrationService : ISyncOrchestrationService
{
    private readonly ConcurrentDictionary<Guid, SyncEnvelope> _envelopes = new();
    private readonly ConcurrentDictionary<Guid, SyncConflict> _conflicts = new();

    public Task<SyncEnvelope> RegisterEnvelopeAsync(
        Guid envelopeId,
        Guid organizationId,
        Guid userId,
        IEnumerable<SyncItem> items,
        CancellationToken cancellationToken = default)
    {
        var envelope = new SyncEnvelope
        {
            EnvelopeId = envelopeId == Guid.Empty ? Guid.NewGuid() : envelopeId,
            OrganizationId = organizationId,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            AppliedAtUtc = null,
            Items = items?.ToList() ?? new List<SyncItem>()
        };

        _envelopes[envelope.EnvelopeId] = envelope;
        return Task.FromResult(envelope);
    }

    public Task<IReadOnlyList<SyncEnvelope>> GetPendingEnvelopesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var pending = _envelopes.Values
            .Where(e => e.OrganizationId == organizationId && e.AppliedAtUtc is null)
            .OrderBy(e => e.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<SyncEnvelope>>(pending);
    }

    public Task MarkEnvelopeAsAppliedAsync(
        Guid envelopeId,
        DateTime appliedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (_envelopes.TryGetValue(envelopeId, out var envelope))
        {
            _envelopes[envelopeId] = envelope with { AppliedAtUtc = appliedAtUtc };
        }

        return Task.CompletedTask;
    }

    public Task<SyncConflict> RegisterConflictAsync(
        SyncConflict conflict,
        CancellationToken cancellationToken = default)
    {
        if (conflict is null)
        {
            throw new ArgumentNullException(nameof(conflict));
        }

        var conflictId = conflict.ConflictId == Guid.Empty ? Guid.NewGuid() : conflict.ConflictId;
        var detectedAt = conflict.DetectedAtUtc == default ? DateTime.UtcNow : conflict.DetectedAtUtc;
        var entry = conflict with { ConflictId = conflictId, DetectedAtUtc = detectedAt };
        _conflicts[conflictId] = entry;
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<SyncConflict>> GetConflictsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var conflicts = _conflicts.Values
            .Where(conflict =>
            {
                if (_envelopes.TryGetValue(conflict.EnvelopeId, out var envelope))
                {
                    return envelope.OrganizationId == organizationId;
                }

                return false;
            })
            .OrderByDescending(c => c.DetectedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<SyncConflict>>(conflicts);
    }
}

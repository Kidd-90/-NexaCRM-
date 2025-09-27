using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.Sync;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.Service.Supabase;

namespace NexaCRM.WebClient.Services.SupabaseEnterprise;

/// <summary>
/// Coordinates offline envelopes and conflict tracking using the in-memory Supabase store.
/// </summary>
public sealed class SupabaseSyncOrchestrationService : ISyncOrchestrationService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly SupabaseEnterpriseDataStore _store;
    private readonly ILogger<SupabaseSyncOrchestrationService> _logger;

    public SupabaseSyncOrchestrationService(
        SupabaseClientProvider clientProvider,
        SupabaseEnterpriseDataStore store,
        ILogger<SupabaseSyncOrchestrationService> logger)
    {
        _clientProvider = clientProvider;
        _store = store;
        _logger = logger;
    }

    public async Task<SyncEnvelope> RegisterEnvelopeAsync(
        Guid envelopeId,
        Guid organizationId,
        Guid userId,
        IEnumerable<SyncItem> items,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(items);

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        var resolvedId = envelopeId == Guid.Empty ? Guid.NewGuid() : envelopeId;
        var materializedItems = items
            .Select(item => item with { ItemId = item.ItemId == Guid.Empty ? Guid.NewGuid() : item.ItemId })
            .ToList();

        var envelope = new SyncEnvelope
        {
            EnvelopeId = resolvedId,
            OrganizationId = organizationId,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            Items = materializedItems
        };

        _store.SyncEnvelopes[resolvedId] = envelope;
        return envelope;
    }

    public Task<IReadOnlyList<SyncEnvelope>> GetPendingEnvelopesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        var envelopes = _store.SyncEnvelopes.Values
            .Where(envelope => envelope.OrganizationId == organizationId && envelope.AppliedAtUtc is null)
            .OrderBy(envelope => envelope.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<SyncEnvelope>>(envelopes);
    }

    public Task MarkEnvelopeAsAppliedAsync(
        Guid envelopeId,
        DateTime appliedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (!_store.SyncEnvelopes.TryGetValue(envelopeId, out var envelope))
        {
            throw new InvalidOperationException($"Envelope {envelopeId} could not be found.");
        }

        _store.SyncEnvelopes[envelopeId] = envelope with { AppliedAtUtc = appliedAtUtc };
        return Task.CompletedTask;
    }

    public Task<SyncConflict> RegisterConflictAsync(
        SyncConflict conflict,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conflict);
        if (conflict.EnvelopeId == Guid.Empty)
        {
            throw new ArgumentException("Envelope id cannot be empty.", nameof(conflict.EnvelopeId));
        }

        var resolved = conflict with
        {
            ConflictId = conflict.ConflictId == Guid.Empty ? Guid.NewGuid() : conflict.ConflictId,
            DetectedAtUtc = conflict.DetectedAtUtc == default ? DateTime.UtcNow : conflict.DetectedAtUtc
        };

        var list = _store.SyncConflicts.GetOrAdd(resolved.EnvelopeId, _ => new List<SyncConflict>());
        list.Add(resolved);

        return Task.FromResult(resolved);
    }

    public Task<IReadOnlyList<SyncConflict>> GetConflictsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        var conflicts = _store.SyncEnvelopes.Values
            .Where(envelope => envelope.OrganizationId == organizationId)
            .SelectMany(envelope => _store.SyncConflicts.TryGetValue(envelope.EnvelopeId, out var list)
                ? (IEnumerable<SyncConflict>)list
                : Array.Empty<SyncConflict>())
            .OrderByDescending(conflict => conflict.DetectedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<SyncConflict>>(conflicts);
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

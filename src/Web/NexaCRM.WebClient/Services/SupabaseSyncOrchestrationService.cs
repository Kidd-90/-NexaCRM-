using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Models.Sync;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseSyncOrchestrationService : ISyncOrchestrationService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseSyncOrchestrationService> _logger;

    public SupabaseSyncOrchestrationService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseSyncOrchestrationService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<SyncPlan> BuildSyncPlanAsync(
        Guid userId,
        SyncPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var since = DateTime.UtcNow.Subtract(policy.RefreshInterval);

            var query = client.From<SyncItemRecord>()
                .Filter(x => x.LastModifiedAt, PostgrestOperator.GreaterThanOrEqual, since)
                .Order(x => x.LastModifiedAt, PostgrestOrdering.Ascending);

            if (policy.Entities is { Count: > 0 })
            {
                query = query.Filter(x => x.EntityType, PostgrestOperator.In, policy.Entities.ToArray());
            }

            var response = await query.Get(cancellationToken: cancellationToken);

            var items = response.Models
                .Select(record => new SyncItem
                {
                    EntityType = record.EntityType,
                    EntityId = record.EntityId,
                    LastModifiedAt = record.LastModifiedAt,
                    PayloadJson = record.PayloadJson
                })
                .ToList();

            return new SyncPlan
            {
                PlanId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                PendingItems = items
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build sync plan for user {UserId}.", userId);
            throw;
        }
    }

    public async Task RecordClientEnvelopeAsync(
        SyncEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (envelope.UserId == Guid.Empty)
        {
            throw new ArgumentException("Envelope must include the user identifier.", nameof(envelope));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var envelopeRecord = new SyncEnvelopeRecord
            {
                Id = envelope.EnvelopeId == Guid.Empty ? Guid.NewGuid() : envelope.EnvelopeId,
                UserId = envelope.UserId,
                GeneratedAt = envelope.GeneratedAt == default ? DateTime.UtcNow : envelope.GeneratedAt
            };

            await client.From<SyncEnvelopeRecord>().Insert(envelopeRecord, cancellationToken: cancellationToken);

            if (envelope.Items.Count > 0)
            {
                var itemRecords = envelope.Items.Select(item => new SyncItemRecord
                {
                    Id = Guid.NewGuid(),
                    EnvelopeId = envelopeRecord.Id,
                    EntityType = item.EntityType,
                    EntityId = item.EntityId,
                    LastModifiedAt = item.LastModifiedAt,
                    PayloadJson = item.PayloadJson
                });

                await client.From<SyncItemRecord>().Insert(itemRecords, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist client envelope {EnvelopeId}.", envelope.EnvelopeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<SyncConflict>> GetConflictsAsync(
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
            var response = await client.From<SyncConflictRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Order(x => x.CreatedAt, PostgrestOrdering.Ascending)
                .Get(cancellationToken: cancellationToken);

            return response.Models
                .Select(record => new SyncConflict
                {
                    ConflictId = record.Id,
                    EntityType = record.EntityType,
                    EntityId = record.EntityId,
                    ResolutionStrategy = record.ResolutionStrategy,
                    PayloadJson = record.PayloadJson
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch sync conflicts for {UserId}.", userId);
            throw;
        }
    }

    public async Task ResolveConflictsAsync(
        IReadOnlyCollection<SyncConflict> conflicts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conflicts);
        if (conflicts.Count == 0)
        {
            return;
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            foreach (var conflict in conflicts)
            {
                if (conflict.ConflictId == Guid.Empty)
                {
                    continue;
                }

                await client.From<SyncConflictRecord>()
                    .Filter(x => x.Id, PostgrestOperator.Equals, conflict.ConflictId)
                    .Delete(cancellationToken: cancellationToken);

                var resolutionEvent = new IntegrationEventRecord
                {
                    Id = Guid.NewGuid(),
                    EventType = "sync.conflict.resolved",
                    PayloadJson = JsonConvert.SerializeObject(new
                    {
                        conflict.ConflictId,
                        conflict.EntityType,
                        conflict.EntityId,
                        conflict.ResolutionStrategy,
                        ResolvedAt = DateTime.UtcNow
                    }),
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<IntegrationEventRecord>().Insert(resolutionEvent, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve sync conflicts.");
            throw;
        }
    }
}

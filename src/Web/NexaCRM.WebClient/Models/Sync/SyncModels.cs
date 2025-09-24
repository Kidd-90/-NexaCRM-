using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Sync;

public sealed class SyncEnvelope
{
    public Guid EnvelopeId { get; init; }

    public Guid UserId { get; init; }

    public DateTime GeneratedAt { get; init; }

    public IReadOnlyCollection<SyncItem> Items { get; init; } = Array.Empty<SyncItem>();
}

public sealed class SyncItem
{
    public string EntityType { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;

    public DateTime LastModifiedAt { get; init; }

    public string PayloadJson { get; init; } = string.Empty;
}

public sealed class SyncPlan
{
    public Guid PlanId { get; init; }

    public DateTime CreatedAt { get; init; }

    public IReadOnlyCollection<SyncItem> PendingItems { get; init; } = Array.Empty<SyncItem>();
}

public sealed class SyncConflict
{
    public Guid ConflictId { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;

    public string ResolutionStrategy { get; init; } = "ServerWins";

    public string PayloadJson { get; init; } = string.Empty;
}

public sealed class SyncPolicy
{
    public TimeSpan RefreshInterval { get; init; } = TimeSpan.FromMinutes(5);

    public int MaxOfflineHours { get; init; } = 12;

    public IReadOnlyCollection<string> Entities { get; init; } = Array.Empty<string>();
}

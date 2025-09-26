using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Sync;

/// <summary>
/// Represents an offline envelope grouping multiple entities for synchronization.
/// </summary>
public sealed record SyncEnvelope
{
    public Guid EnvelopeId { get; init; }

    public Guid OrganizationId { get; init; }

    public Guid UserId { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? AppliedAtUtc { get; init; }

    public IReadOnlyCollection<SyncItem> Items { get; init; } = Array.Empty<SyncItem>();
}

/// <summary>
/// Represents a single item inside a sync envelope.
/// </summary>
public sealed record SyncItem
{
    public Guid ItemId { get; init; }

    public Guid EnvelopeId { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;

    public string PayloadJson { get; init; } = string.Empty;

    public DateTime UpdatedAtUtc { get; init; }
}

/// <summary>
/// Describes a conflict detected during synchronization.
/// </summary>
public sealed record SyncConflict
{
    public Guid ConflictId { get; init; }

    public Guid EnvelopeId { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;

    public string ResolutionState { get; init; } = "pending";

    public string? ResolutionNotes { get; init; }

    public DateTime DetectedAtUtc { get; init; }
}

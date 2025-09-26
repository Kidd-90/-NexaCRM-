using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.FileHub;

/// <summary>
/// Represents a document registered inside the file hub.
/// </summary>
public sealed record FileDocument
{
    public Guid DocumentId { get; init; }

    public Guid OrganizationId { get; init; }

    public Guid OwnerId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Represents a single stored version of a document.
/// </summary>
public sealed record FileVersion
{
    public Guid VersionId { get; init; }

    public Guid DocumentId { get; init; }

    public string StoragePath { get; init; } = string.Empty;

    public string ContentHash { get; init; } = string.Empty;

    public long SizeInBytes { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public Guid CreatedBy { get; init; }
}

/// <summary>
/// Represents a collaboration thread tied to a document or workflow.
/// </summary>
public sealed record CommunicationThread
{
    public Guid ThreadId { get; init; }

    public Guid OrganizationId { get; init; }

    public string Topic { get; init; } = string.Empty;

    public IReadOnlyCollection<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();

    public DateTime CreatedAtUtc { get; init; }
}

/// <summary>
/// Represents a message delivered through one of the supported channels.
/// </summary>
public sealed record ThreadMessage
{
    public Guid MessageId { get; init; }

    public Guid ThreadId { get; init; }

    public Guid AuthorId { get; init; }

    public string Body { get; init; } = string.Empty;

    public string Channel { get; init; } = "internal";

    public DateTime SentAtUtc { get; init; }
}

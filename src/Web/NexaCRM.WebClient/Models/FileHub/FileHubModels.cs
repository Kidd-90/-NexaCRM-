using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.FileHub;

public sealed class FileUploadRequest
{
    public Guid OwnerId { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public string EntityId { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/octet-stream";

    public long ContentLength { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}

public sealed class FileUploadUrl
{
    public Uri UploadUrl { get; init; } = new("https://localhost/upload");

    public string ObjectPath { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> RequiredHeaders { get; init; }
        = new Dictionary<string, string>();
}

public sealed class FileMetadata
{
    public Guid FileId { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long Size { get; init; }

    public DateTime UploadedAt { get; init; }

    public Guid UploadedBy { get; init; }

    public string StoragePath { get; init; } = string.Empty;

    public IReadOnlyList<FileVersion> Versions { get; init; } = Array.Empty<FileVersion>();
}

public sealed class FileVersion
{
    public Guid VersionId { get; init; }

    public string StoragePath { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public Guid CreatedBy { get; init; }

    public string Notes { get; init; } = string.Empty;
}

public sealed class CommunicationThread
{
    public Guid ThreadId { get; init; }

    public string Channel { get; init; } = string.Empty;

    public IReadOnlyList<ThreadMessage> Messages { get; init; } = Array.Empty<ThreadMessage>();
}

public sealed class ThreadMessage
{
    public Guid MessageId { get; init; }

    public Guid SenderId { get; init; }

    public string Body { get; init; } = string.Empty;

    public DateTime SentAt { get; init; }

    public IReadOnlyCollection<string> DeliveryChannels { get; init; }
        = Array.Empty<string>();
}

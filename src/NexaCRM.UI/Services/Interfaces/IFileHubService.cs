using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.FileHub;

namespace NexaCRM.WebClient.Services.Interfaces;

/// <summary>
/// Provides APIs for registering documents, managing versions, and collaborating around files.
/// </summary>
public interface IFileHubService
{
    Task<FileDocument> CreateDocumentAsync(
        Guid organizationId,
        Guid ownerId,
        string name,
        string category,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken = default);

    Task<FileVersion> AddVersionAsync(
        Guid documentId,
        Guid authorId,
        string storagePath,
        string contentHash,
        long sizeInBytes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileDocument>> GetDocumentsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileVersion>> GetDocumentVersionsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<CommunicationThread> StartThreadAsync(
        Guid documentId,
        string topic,
        IEnumerable<Guid> participantIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommunicationThread>> GetThreadsForDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<ThreadMessage> AppendMessageAsync(
        Guid threadId,
        Guid authorId,
        string channel,
        string body,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThreadMessage>> GetThreadMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default);
}

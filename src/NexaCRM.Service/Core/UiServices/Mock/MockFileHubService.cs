using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.FileHub;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock;

public sealed class MockFileHubService : IFileHubService
{
    private readonly ConcurrentDictionary<Guid, FileDocument> _documents = new();
    private readonly ConcurrentDictionary<Guid, List<FileVersion>> _versions = new();
    private readonly ConcurrentDictionary<Guid, List<Guid>> _documentThreads = new();
    private readonly ConcurrentDictionary<Guid, CommunicationThread> _threads = new();
    private readonly ConcurrentDictionary<Guid, List<ThreadMessage>> _threadMessages = new();

    public Task<FileDocument> CreateDocumentAsync(
        Guid organizationId,
        Guid ownerId,
        string name,
        string category,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken = default)
    {
        var document = new FileDocument
        {
            DocumentId = Guid.NewGuid(),
            OrganizationId = organizationId,
            OwnerId = ownerId,
            Name = name ?? string.Empty,
            Category = category ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Tags = tags ?? new Dictionary<string, string>()
        };

        _documents[document.DocumentId] = document;
        _versions[document.DocumentId] = new List<FileVersion>();
        _documentThreads[document.DocumentId] = new List<Guid>();
        return Task.FromResult(document);
    }

    public Task<FileVersion> AddVersionAsync(
        Guid documentId,
        Guid authorId,
        string storagePath,
        string contentHash,
        long sizeInBytes,
        CancellationToken cancellationToken = default)
    {
        if (!_documents.ContainsKey(documentId))
        {
            throw new InvalidOperationException($"Document {documentId} not found.");
        }

        var version = new FileVersion
        {
            VersionId = Guid.NewGuid(),
            DocumentId = documentId,
            StoragePath = storagePath ?? string.Empty,
            ContentHash = contentHash ?? string.Empty,
            SizeInBytes = sizeInBytes,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = authorId
        };

        var list = _versions.GetOrAdd(documentId, _ => new List<FileVersion>());
        list.Add(version);

        _documents.AddOrUpdate(
            documentId,
            _ => throw new InvalidOperationException(),
            (_, doc) => doc with { UpdatedAtUtc = DateTime.UtcNow });

        return Task.FromResult(version);
    }

    public Task<IReadOnlyList<FileDocument>> GetDocumentsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var docs = _documents.Values
            .Where(doc => doc.OrganizationId == organizationId)
            .OrderByDescending(doc => doc.UpdatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<FileDocument>>(docs);
    }

    public Task<IReadOnlyList<FileVersion>> GetDocumentVersionsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var versions = _versions.TryGetValue(documentId, out var list)
            ? list.OrderByDescending(v => v.CreatedAtUtc).ToList()
            : new List<FileVersion>();

        return Task.FromResult<IReadOnlyList<FileVersion>>(versions);
    }

    public Task<CommunicationThread> StartThreadAsync(
        Guid documentId,
        string topic,
        IEnumerable<Guid> participantIds,
        CancellationToken cancellationToken = default)
    {
        if (!_documents.ContainsKey(documentId))
        {
            throw new InvalidOperationException($"Document {documentId} not found.");
        }

        var thread = new CommunicationThread
        {
            ThreadId = Guid.NewGuid(),
            OrganizationId = _documents[documentId].OrganizationId,
            Topic = topic ?? string.Empty,
            ParticipantIds = participantIds?.ToArray() ?? Array.Empty<Guid>(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _threads[thread.ThreadId] = thread;
        _threadMessages[thread.ThreadId] = new List<ThreadMessage>();
        _documentThreads.AddOrUpdate(documentId, _ => new List<Guid> { thread.ThreadId }, (_, list) =>
        {
            list.Add(thread.ThreadId);
            return list;
        });

        return Task.FromResult(thread);
    }

    public Task<IReadOnlyList<CommunicationThread>> GetThreadsForDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        if (!_documentThreads.TryGetValue(documentId, out var ids))
        {
            return Task.FromResult<IReadOnlyList<CommunicationThread>>(Array.Empty<CommunicationThread>());
        }

        var threads = ids
            .Select(id => _threads.TryGetValue(id, out var thread) ? thread : null)
            .Where(thread => thread is not null)
            .Select(thread => thread!)
            .OrderByDescending(thread => thread.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<CommunicationThread>>(threads);
    }

    public Task<ThreadMessage> AppendMessageAsync(
        Guid threadId,
        Guid authorId,
        string channel,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (!_threads.ContainsKey(threadId))
        {
            throw new InvalidOperationException($"Thread {threadId} not found.");
        }

        var message = new ThreadMessage
        {
            MessageId = Guid.NewGuid(),
            ThreadId = threadId,
            AuthorId = authorId,
            Channel = string.IsNullOrWhiteSpace(channel) ? "internal" : channel,
            Body = body ?? string.Empty,
            SentAtUtc = DateTime.UtcNow
        };

        _threadMessages.AddOrUpdate(
            threadId,
            _ => new List<ThreadMessage> { message },
            (_, list) =>
            {
                list.Add(message);
                return list;
            });

        return Task.FromResult(message);
    }

    public Task<IReadOnlyList<ThreadMessage>> GetThreadMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
    {
        var messages = _threadMessages.TryGetValue(threadId, out var list)
            ? list.OrderBy(msg => msg.SentAtUtc).ToList()
            : new List<ThreadMessage>();

        return Task.FromResult<IReadOnlyList<ThreadMessage>>(messages);
    }
}

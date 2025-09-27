using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.FileHub;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.Service.Supabase;

namespace NexaCRM.WebClient.Services.SupabaseEnterprise;

/// <summary>
/// Provides document registration and collaboration features backed by a Supabase style store.
/// </summary>
public sealed class SupabaseFileHubService : IFileHubService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly SupabaseEnterpriseDataStore _store;
    private readonly ILogger<SupabaseFileHubService> _logger;

    public SupabaseFileHubService(
        SupabaseClientProvider clientProvider,
        SupabaseEnterpriseDataStore store,
        ILogger<SupabaseFileHubService> logger)
    {
        _clientProvider = clientProvider;
        _store = store;
        _logger = logger;
    }

    public async Task<FileDocument> CreateDocumentAsync(
        Guid organizationId,
        Guid ownerId,
        string name,
        string category,
        IReadOnlyDictionary<string, string>? tags,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id cannot be empty.", nameof(ownerId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var document = new FileDocument
        {
            DocumentId = Guid.NewGuid(),
            OrganizationId = organizationId,
            OwnerId = ownerId,
            Name = name,
            Category = category,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Tags = tags ?? new Dictionary<string, string>()
        };

        _store.Documents[document.DocumentId] = document;
        _store.DocumentVersions.TryAdd(document.DocumentId, new List<FileVersion>());
        _store.DocumentThreads.TryAdd(document.DocumentId, new List<Guid>());

        return document;
    }

    public async Task<FileVersion> AddVersionAsync(
        Guid documentId,
        Guid authorId,
        string storagePath,
        string contentHash,
        long sizeInBytes,
        CancellationToken cancellationToken = default)
    {
        if (!_store.Documents.TryGetValue(documentId, out var document))
        {
            throw new InvalidOperationException($"Document {documentId} was not found.");
        }

        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("Author id cannot be empty.", nameof(authorId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        var version = new FileVersion
        {
            VersionId = Guid.NewGuid(),
            DocumentId = documentId,
            CreatedBy = authorId,
            StoragePath = storagePath,
            ContentHash = contentHash,
            SizeInBytes = sizeInBytes,
            CreatedAtUtc = DateTime.UtcNow
        };

        var versions = _store.DocumentVersions.GetOrAdd(documentId, _ => new List<FileVersion>());
        versions.Add(version);
        _store.Documents[documentId] = document with { UpdatedAtUtc = version.CreatedAtUtc };

        return version;
    }

    public Task<IReadOnlyList<FileDocument>> GetDocumentsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        var documents = _store.Documents.Values
            .Where(d => d.OrganizationId == organizationId)
            .OrderByDescending(d => d.UpdatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<FileDocument>>(documents);
    }

    public Task<IReadOnlyList<FileVersion>> GetDocumentVersionsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        if (!_store.DocumentVersions.TryGetValue(documentId, out var versions))
        {
            return Task.FromResult<IReadOnlyList<FileVersion>>(Array.Empty<FileVersion>());
        }

        return Task.FromResult<IReadOnlyList<FileVersion>>(versions
            .OrderByDescending(v => v.CreatedAtUtc)
            .ToList());
    }

    public async Task<CommunicationThread> StartThreadAsync(
        Guid documentId,
        string topic,
        IEnumerable<Guid> participantIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participantIds);
        if (!_store.Documents.ContainsKey(documentId))
        {
            throw new InvalidOperationException($"Document {documentId} was not found.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        var document = _store.Documents[documentId];
        var participants = participantIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var thread = new CommunicationThread
        {
            ThreadId = Guid.NewGuid(),
            OrganizationId = document.OrganizationId,
            Topic = topic,
            ParticipantIds = participants,
            CreatedAtUtc = DateTime.UtcNow
        };

        _store.Threads[thread.ThreadId] = thread;
        _store.ThreadMessages.TryAdd(thread.ThreadId, new List<ThreadMessage>());

        var list = _store.DocumentThreads.GetOrAdd(documentId, _ => new List<Guid>());
        list.Add(thread.ThreadId);

        return thread;
    }

    public Task<IReadOnlyList<CommunicationThread>> GetThreadsForDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        if (!_store.DocumentThreads.TryGetValue(documentId, out var threadIds))
        {
            return Task.FromResult<IReadOnlyList<CommunicationThread>>(Array.Empty<CommunicationThread>());
        }

        var threads = threadIds
            .Where(id => _store.Threads.ContainsKey(id))
            .Select(id => _store.Threads[id])
            .OrderByDescending(t => t.CreatedAtUtc)
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
        if (!_store.Threads.TryGetValue(threadId, out _))
        {
            throw new InvalidOperationException($"Thread {threadId} was not found.");
        }

        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("Author id cannot be empty.", nameof(authorId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        var message = new ThreadMessage
        {
            MessageId = Guid.NewGuid(),
            ThreadId = threadId,
            AuthorId = authorId,
            Channel = channel,
            Body = body,
            SentAtUtc = DateTime.UtcNow
        };

        var list = _store.ThreadMessages.GetOrAdd(threadId, _ => new List<ThreadMessage>());
        list.Add(message);

        return Task.FromResult(message);
    }

    public Task<IReadOnlyList<ThreadMessage>> GetThreadMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
    {
        if (!_store.ThreadMessages.TryGetValue(threadId, out var messages))
        {
            return Task.FromResult<IReadOnlyList<ThreadMessage>>(Array.Empty<ThreadMessage>());
        }

        var ordered = messages
            .OrderBy(m => m.SentAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<ThreadMessage>>(ordered);
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

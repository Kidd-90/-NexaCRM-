using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Service.Supabase;
using NexaCRM.UI.Models.FileHub;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.Service.Supabase.Enterprise;

/// <summary>
/// Lightweight multi-channel hub storing conversation metadata through the shared Supabase store.
/// </summary>
public sealed class SupabaseCommunicationHubService : ICommunicationHubService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly SupabaseEnterpriseDataStore _store;
    private readonly ILogger<SupabaseCommunicationHubService> _logger;

    public SupabaseCommunicationHubService(
        SupabaseClientProvider clientProvider,
        SupabaseEnterpriseDataStore store,
        ILogger<SupabaseCommunicationHubService> logger)
    {
        _clientProvider = clientProvider;
        _store = store;
        _logger = logger;
    }

    public async Task<CommunicationThread> StartThreadAsync(
        Guid organizationId,
        string topic,
        IEnumerable<Guid> participantIds,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(participantIds);

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        var participants = participantIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var thread = new CommunicationThread
        {
            ThreadId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Topic = topic,
            ParticipantIds = participants,
            CreatedAtUtc = DateTime.UtcNow
        };

        _store.Threads[thread.ThreadId] = thread;
        _store.ThreadMessages.TryAdd(thread.ThreadId, new List<ThreadMessage>());

        return thread;
    }

    public Task<IReadOnlyList<CommunicationThread>> GetThreadsForParticipantAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        if (participantId == Guid.Empty)
        {
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));
        }

        var threads = _store.Threads.Values
            .Where(thread => thread.ParticipantIds.Contains(participantId))
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
        if (!_store.Threads.ContainsKey(threadId))
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

    public Task<IReadOnlyList<ThreadMessage>> GetMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
    {
        if (!_store.ThreadMessages.TryGetValue(threadId, out var messages))
        {
            return Task.FromResult<IReadOnlyList<ThreadMessage>>(Array.Empty<ThreadMessage>());
        }

        var ordered = messages
            .OrderBy(message => message.SentAtUtc)
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

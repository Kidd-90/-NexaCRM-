using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.FileHub;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock;

public sealed class MockCommunicationHubService : ICommunicationHubService
{
    private readonly ConcurrentDictionary<Guid, CommunicationThread> _threads = new();
    private readonly ConcurrentDictionary<Guid, List<ThreadMessage>> _messages = new();

    public Task<CommunicationThread> StartThreadAsync(
        Guid organizationId,
        string topic,
        IEnumerable<Guid> participantIds,
        CancellationToken cancellationToken = default)
    {
        var thread = new CommunicationThread
        {
            ThreadId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Topic = topic ?? string.Empty,
            ParticipantIds = participantIds?.ToArray() ?? Array.Empty<Guid>(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _threads[thread.ThreadId] = thread;
        _messages[thread.ThreadId] = new List<ThreadMessage>();
        return Task.FromResult(thread);
    }

    public Task<IReadOnlyList<CommunicationThread>> GetThreadsForParticipantAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        var threads = _threads.Values
            .Where(t => t.ParticipantIds.Contains(participantId))
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

        _messages.AddOrUpdate(
            threadId,
            _ => new List<ThreadMessage> { message },
            (_, list) =>
            {
                list.Add(message);
                return list;
            });

        return Task.FromResult(message);
    }

    public Task<IReadOnlyList<ThreadMessage>> GetMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default)
    {
        var entries = _messages.TryGetValue(threadId, out var list)
            ? list.OrderBy(m => m.SentAtUtc).ToList()
            : new List<ThreadMessage>();

        return Task.FromResult<IReadOnlyList<ThreadMessage>>(entries);
    }
}

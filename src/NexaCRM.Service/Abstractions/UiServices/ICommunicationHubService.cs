using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.FileHub;

namespace NexaCRM.UI.Services.Interfaces;

/// <summary>
/// Exposes multi-channel communication primitives backed by Supabase.
/// </summary>
public interface ICommunicationHubService
{
    Task<CommunicationThread> StartThreadAsync(
        Guid organizationId,
        string topic,
        IEnumerable<Guid> participantIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommunicationThread>> GetThreadsForParticipantAsync(
        Guid participantId,
        CancellationToken cancellationToken = default);

    Task<ThreadMessage> AppendMessageAsync(
        Guid threadId,
        Guid authorId,
        string channel,
        string body,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThreadMessage>> GetMessagesAsync(
        Guid threadId,
        CancellationToken cancellationToken = default);
}

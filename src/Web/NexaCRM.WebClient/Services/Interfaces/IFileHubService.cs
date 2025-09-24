using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.FileHub;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IFileHubService
{
    Task<FileUploadUrl> CreateUploadUrlAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<FileMetadata> RegisterUploadAsync(
        Guid userId,
        string objectPath,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileVersion>> GetFileVersionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    Task<CommunicationThread> EnsureThreadAsync(
        string entityType,
        string entityId,
        string channel,
        CancellationToken cancellationToken = default);

    Task<ThreadMessage> SendThreadMessageAsync(
        Guid threadId,
        Guid senderId,
        string body,
        IEnumerable<string> channels,
        CancellationToken cancellationToken = default);
}

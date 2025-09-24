using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ICommunicationHubService
{
    Task SendEmailAsync(
        Guid senderId,
        IEnumerable<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default);

    Task SendSmsAsync(
        Guid senderId,
        IEnumerable<string> recipients,
        string message,
        CancellationToken cancellationToken = default);

    Task EnqueuePushNotificationAsync(
        Guid userId,
        string title,
        string message,
        CancellationToken cancellationToken = default);
}

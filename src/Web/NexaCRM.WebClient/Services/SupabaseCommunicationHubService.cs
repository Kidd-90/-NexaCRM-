using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseCommunicationHubService : ICommunicationHubService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseCommunicationHubService> _logger;

    public SupabaseCommunicationHubService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseCommunicationHubService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        Guid senderId,
        IEnumerable<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        await EnqueueCommunicationAsync(
            senderId,
            recipients,
            "email",
            new { subject, body },
            cancellationToken);
    }

    public async Task SendSmsAsync(
        Guid senderId,
        IEnumerable<string> recipients,
        string message,
        CancellationToken cancellationToken = default)
    {
        await EnqueueCommunicationAsync(
            senderId,
            recipients,
            "sms",
            new { message },
            cancellationToken);
    }

    public async Task EnqueuePushNotificationAsync(
        Guid userId,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = new IntegrationEventRecord
            {
                Id = Guid.NewGuid(),
                EventType = "notification.push",
                PayloadJson = JsonConvert.SerializeObject(new
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    ScheduledAt = DateTime.UtcNow
                }),
                CreatedAt = DateTime.UtcNow
            };

            await client.From<IntegrationEventRecord>().Insert(record, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue push notification for {UserId}.", userId);
            throw;
        }
    }

    private async Task EnqueueCommunicationAsync(
        Guid senderId,
        IEnumerable<string> recipients,
        string channel,
        object payload,
        CancellationToken cancellationToken)
    {
        if (senderId == Guid.Empty)
        {
            throw new ArgumentException("Sender id cannot be empty.", nameof(senderId));
        }

        ArgumentNullException.ThrowIfNull(recipients);
        var recipientList = recipients
            .Where(recipient => !string.IsNullOrWhiteSpace(recipient))
            .Select(recipient => recipient.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (recipientList.Length == 0)
        {
            throw new ArgumentException("At least one recipient is required.", nameof(recipients));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = new IntegrationEventRecord
            {
                Id = Guid.NewGuid(),
                EventType = $"communication.{channel}",
                PayloadJson = JsonConvert.SerializeObject(new
                {
                    SenderId = senderId,
                    Recipients = recipientList,
                    Payload = payload,
                    RequestedAt = DateTime.UtcNow
                }),
                CreatedAt = DateTime.UtcNow
            };

            await client.From<IntegrationEventRecord>().Insert(record, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue {Channel} communication.", channel);
            throw;
        }
    }
}

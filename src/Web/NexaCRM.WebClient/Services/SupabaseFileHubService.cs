using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NexaCRM.WebClient.Models.FileHub;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Options;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseFileHubService : IFileHubService
{
    private const string StorageBucket = "crm-documents";
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseFileHubService> _logger;
    private readonly string _storageBaseUrl;
    private readonly string _anonKey;

    public SupabaseFileHubService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseFileHubService> logger,
        IOptions<SupabaseClientOptions> optionsAccessor)
    {
        _clientProvider = clientProvider;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(optionsAccessor);
        var options = optionsAccessor.Value;
        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new InvalidOperationException("Supabase Url must be configured to use the file hub service.");
        }

        if (string.IsNullOrWhiteSpace(options.AnonKey))
        {
            throw new InvalidOperationException("Supabase anon key must be configured to use the file hub service.");
        }

        _storageBaseUrl = options.Url.TrimEnd('/') + "/storage/v1";
        _anonKey = options.AnonKey;
    }

    public async Task<FileUploadUrl> CreateUploadUrlAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException("File name is required for uploads.", nameof(request));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var session = client.Auth.CurrentSession ?? await client.Auth.RetrieveSessionAsync();

            var objectPath = BuildObjectPath(request);
            var uploadUrl = new Uri($"{_storageBaseUrl}/object/{StorageBucket}/{objectPath}");

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {session?.AccessToken ?? _anonKey}",
                ["x-upsert"] = "true",
                ["Content-Type"] = request.ContentType
            };

            return new FileUploadUrl
            {
                UploadUrl = uploadUrl,
                ObjectPath = objectPath,
                RequiredHeaders = headers
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Supabase storage upload url for {File}.", request.FileName);
            throw;
        }
    }

    public async Task<FileMetadata> RegisterUploadAsync(
        Guid userId,
        string objectPath,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(objectPath))
        {
            throw new ArgumentException("Object path must be provided after uploading to Supabase storage.", nameof(objectPath));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var document = new FileDocumentRecord
            {
                Id = Guid.NewGuid(),
                OwnerId = request.OwnerId == Guid.Empty ? userId : request.OwnerId,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                FileName = request.FileName,
                ContentType = request.ContentType,
                Size = request.ContentLength,
                StoragePath = objectPath,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };

            await client.From<FileDocumentRecord>().Insert(document, cancellationToken: cancellationToken);

            var version = new FileVersionRecord
            {
                Id = Guid.NewGuid(),
                FileId = document.Id,
                StoragePath = objectPath,
                CreatedAt = document.UploadedAt,
                CreatedBy = userId,
                Notes = JsonConvert.SerializeObject(request.Metadata)
            };

            await client.From<FileVersionRecord>().Insert(version, cancellationToken: cancellationToken);

            await LogFileAuditAsync(client, document.Id, "file.upload", userId, cancellationToken);

            return new FileMetadata
            {
                FileId = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                Size = document.Size,
                UploadedAt = document.UploadedAt,
                UploadedBy = document.UploadedBy,
                StoragePath = document.StoragePath,
                Versions = new[]
                {
                    new FileVersion
                    {
                        VersionId = version.Id,
                        StoragePath = version.StoragePath,
                        CreatedAt = version.CreatedAt,
                        CreatedBy = version.CreatedBy,
                        Notes = version.Notes ?? string.Empty
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register Supabase upload for {File}.", request.FileName);
            throw;
        }
    }

    public async Task<IReadOnlyList<FileVersion>> GetFileVersionsAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        if (fileId == Guid.Empty)
        {
            throw new ArgumentException("File id cannot be empty.", nameof(fileId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<FileVersionRecord>()
                .Filter(x => x.FileId, PostgrestOperator.Equals, fileId)
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Get(cancellationToken: cancellationToken);

            return response.Models
                .Select(record => new FileVersion
                {
                    VersionId = record.Id,
                    StoragePath = record.StoragePath,
                    CreatedAt = record.CreatedAt,
                    CreatedBy = record.CreatedBy,
                    Notes = record.Notes ?? string.Empty
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Supabase file versions for file {FileId}.", fileId);
            throw;
        }
    }

    public async Task<CommunicationThread> EnsureThreadAsync(
        string entityType,
        string entityId,
        string channel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<CommunicationThreadRecord>()
                .Filter(x => x.EntityType, PostgrestOperator.Equals, entityType)
                .Filter(x => x.EntityId, PostgrestOperator.Equals, entityId)
                .Filter(x => x.Channel, PostgrestOperator.Equals, channel)
                .Single(cancellationToken: cancellationToken);

            var thread = response.Model;
            if (thread is null)
            {
                thread = new CommunicationThreadRecord
                {
                    Id = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Channel = channel,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<CommunicationThreadRecord>().Insert(thread, cancellationToken: cancellationToken);
            }

            var messages = await client.From<ThreadMessageRecord>()
                .Filter(x => x.ThreadId, PostgrestOperator.Equals, thread.Id)
                .Order(x => x.SentAt, PostgrestOrdering.Descending)
                .Get(cancellationToken: cancellationToken);

            return new CommunicationThread
            {
                ThreadId = thread.Id,
                Channel = thread.Channel,
                Messages = messages.Models.Select(MapToThreadMessage).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure communication thread for {EntityType}:{EntityId}.", entityType, entityId);
            throw;
        }
    }

    public async Task<ThreadMessage> SendThreadMessageAsync(
        Guid threadId,
        Guid senderId,
        string body,
        IEnumerable<string> channels,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        if (threadId == Guid.Empty)
        {
            throw new ArgumentException("Thread id cannot be empty.", nameof(threadId));
        }

        if (senderId == Guid.Empty)
        {
            throw new ArgumentException("Sender id cannot be empty.", nameof(senderId));
        }

        ArgumentNullException.ThrowIfNull(channels);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var normalizedChannels = channels
                .Where(channel => !string.IsNullOrWhiteSpace(channel))
                .Select(channel => channel.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            var record = new ThreadMessageRecord
            {
                Id = Guid.NewGuid(),
                ThreadId = threadId,
                SenderId = senderId,
                Body = body,
                SentAt = DateTime.UtcNow,
                Channels = string.Join(',', normalizedChannels)
            };

            await client.From<ThreadMessageRecord>().Insert(record, cancellationToken: cancellationToken);
            await LogFileAuditAsync(client, threadId, "thread.message", senderId, cancellationToken);

            await DispatchCommunicationsAsync(client, record, normalizedChannels, cancellationToken);

            return MapToThreadMessage(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send communication message on thread {ThreadId}.", threadId);
            throw;
        }
    }

    private static string BuildObjectPath(FileUploadRequest request)
    {
        var safeName = request.FileName
            .Replace(' ', '-')
            .ToLowerInvariant();

        var datePart = DateTime.UtcNow.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        var identifier = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

        return $"{request.EntityType}/{request.EntityId}/{datePart}/{identifier}-{safeName}";
    }

    private static ThreadMessage MapToThreadMessage(ThreadMessageRecord record)
    {
        var channels = string.IsNullOrWhiteSpace(record.Channels)
            ? Array.Empty<string>()
            : record.Channels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ThreadMessage
        {
            MessageId = record.Id,
            SenderId = record.SenderId,
            Body = record.Body,
            SentAt = record.SentAt,
            DeliveryChannels = channels
        };
    }

    private static async Task LogFileAuditAsync(
        Supabase.Client client,
        Guid entityId,
        string action,
        Guid actorId,
        CancellationToken cancellationToken)
    {
        var auditRecord = new AuditLogRecord
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            Action = action,
            EntityType = "file",
            EntityId = entityId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await client.From<AuditLogRecord>().Insert(auditRecord, cancellationToken: cancellationToken);
    }

    private async Task DispatchCommunicationsAsync(
        Supabase.Client client,
        ThreadMessageRecord record,
        IReadOnlyCollection<string> channels,
        CancellationToken cancellationToken)
    {
        if (channels.Count == 0)
        {
            return;
        }

        var events = channels.Select(channel => new IntegrationEventRecord
        {
            Id = Guid.NewGuid(),
            EventType = $"communication.{channel}",
            PayloadJson = JsonConvert.SerializeObject(new
            {
                record.ThreadId,
                record.SenderId,
                record.Body,
                record.SentAt,
                Channels = channels
            }),
            CreatedAt = DateTime.UtcNow
        });

        await client.From<IntegrationEventRecord>().Insert(events, cancellationToken: cancellationToken);
    }
}

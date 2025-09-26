using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexaCRM.Services.Admin.Models.Settings;
using NexaCRM.Services.Admin.Models.Sms;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.Services.Admin.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using Supabase.Postgrest.Exceptions;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseSmsService : ISmsService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseSmsService> _logger;

    public SupabaseSmsService(SupabaseClientProvider clientProvider, ILogger<SupabaseSmsService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task SendBulkAsync(IEnumerable<BulkSmsRequest> batches, IProgress<int>? progress = null)
    {
        if (batches is null)
        {
            throw new ArgumentNullException(nameof(batches));
        }

        var batchList = batches.ToList();
        if (batchList.Count == 0)
        {
            progress?.Report(100);
            return;
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);
            var settings = await LoadSettingsAsync(client, userId) ?? new SmsSettings();
            var defaultSender = settings.SenderNumbers.FirstOrDefault() ?? settings.SenderId ?? string.Empty;

            var historyRecords = new List<SmsHistoryRecord>();
            var eventPayloads = new List<object>();

            foreach (var (request, index) in batchList.Select((value, idx) => (value, idx)))
            {
                foreach (var recipient in request.Recipients)
                {
                    historyRecords.Add(new SmsHistoryRecord
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Recipient = recipient,
                        RecipientName = string.Empty,
                        Message = request.Message,
                        SentAt = DateTime.UtcNow,
                        Status = "Queued",
                        SenderNumber = defaultSender,
                        AttachmentsJson = null,
                        MetadataJson = null
                    });
                }

                eventPayloads.Add(new
                {
                    Request = request,
                    RequestedAt = DateTime.UtcNow
                });

                progress?.Report((index + 1) * 100 / batchList.Count);
            }

            if (historyRecords.Count > 0)
            {
                await client.From<SmsHistoryRecord>().Insert(historyRecords);
            }

            foreach (var payload in eventPayloads)
            {
                await CreateIntegrationEventAsync(client, "sms.dispatch.requested", payload);
            }

            await CreateAuditLogAsync(client, userId, "sms.send.bulk", JsonConvert.SerializeObject(batchList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk SMS via Supabase.");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetSenderNumbersAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var response = await client.From<SmsSenderRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Order(x => x.CreatedAt, PostgrestOrdering.Ascending)
                .Get();

            return response.Models.Select(record => record.Number).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SMS sender numbers from Supabase.");
            throw;
        }
    }

    public async Task SaveSenderNumberAsync(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return;
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var normalized = number.Trim();
            var existing = await client.From<SmsSenderRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Filter(x => x.Number, PostgrestOperator.Equals, normalized)
                .Get();

            if (existing.Models.Count > 0)
            {
                return;
            }

            var record = new SmsSenderRecord
            {
                UserId = userId,
                Number = normalized,
                CreatedAt = DateTime.UtcNow
            };

            await client.From<SmsSenderRecord>().Insert(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save SMS sender number to Supabase.");
            throw;
        }
    }

    public async Task DeleteSenderNumberAsync(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return;
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            await client.From<SmsSenderRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Filter(x => x.Number, PostgrestOperator.Equals, number.Trim())
                .Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete SMS sender number from Supabase.");
            throw;
        }
    }

    public async Task<SmsSettings?> GetSettingsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);
            return await LoadSettingsAsync(client, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SMS settings from Supabase.");
            throw;
        }
    }

    public async Task SaveSettingsAsync(SmsSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var record = new SmsSettingsRecord
            {
                UserId = userId,
                ProviderApiKey = settings.ProviderApiKey,
                ProviderApiSecret = settings.ProviderApiSecret,
                SenderId = settings.SenderId,
                DefaultTemplate = settings.DefaultTemplate,
                UpdatedAt = DateTime.UtcNow
            };

            await client.From<SmsSettingsRecord>()
                .Upsert(record);

            var existingNumbers = await client.From<SmsSenderRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Get();

            var existingSet = existingNumbers.Models
                .Select(r => r.Number)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var desiredNumbers = settings.SenderNumbers
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Where(n => n.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var number in desiredNumbers)
            {
                if (!existingSet.Contains(number))
                {
                    await SaveSenderNumberAsync(number);
                }
            }

            foreach (var recordNumber in existingNumbers.Models)
            {
                if (!desiredNumbers.Contains(recordNumber.Number))
                {
                    await client.From<SmsSenderRecord>()
                        .Filter(x => x.Id, PostgrestOperator.Equals, recordNumber.Id)
                        .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                        .Delete();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save SMS settings to Supabase.");
            throw;
        }
    }

    public async Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var response = await client.From<SmsHistoryRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Order(x => x.SentAt, PostgrestOrdering.Descending)
                .Get();

            return response.Models.Select(MapToHistoryItem).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SMS history from Supabase.");
            throw;
        }
    }

    public async Task ScheduleAsync(SmsScheduleItem schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var record = new SmsScheduleRecord
            {
                Id = schedule.Id == Guid.Empty ? Guid.NewGuid() : schedule.Id,
                UserId = userId,
                ScheduledAt = schedule.ScheduledAt,
                PayloadJson = JsonConvert.SerializeObject(schedule.Request),
                IsCancelled = schedule.IsCancelled,
                Status = schedule.IsCancelled ? "cancelled" : "scheduled",
                CreatedAt = DateTime.UtcNow
            };

            await client.From<SmsScheduleRecord>().Upsert(record);

            await CreateIntegrationEventAsync(client, "sms.schedule.created", new
            {
                ScheduleId = record.Id,
                record.ScheduledAt,
                schedule.Request
            });

            await CreateAuditLogAsync(client, userId, "sms.schedule", JsonConvert.SerializeObject(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule SMS via Supabase.");
            throw;
        }
    }

    public async Task<IEnumerable<SmsScheduleItem>> GetUpcomingSchedulesAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var response = await client.From<SmsScheduleRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Filter(x => x.IsCancelled, PostgrestOperator.Equals, false)
                .Order(x => x.ScheduledAt, PostgrestOrdering.Ascending)
                .Get();

            return response.Models.Select(MapToScheduleItem).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SMS schedules from Supabase.");
            throw;
        }
    }

    public async Task CancelAsync(Guid id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var record = new SmsScheduleRecord
            {
                IsCancelled = true,
                Status = "cancelled"
            };

            await client.From<SmsScheduleRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Update(record);

            await CreateIntegrationEventAsync(client, "sms.schedule.cancelled", new { ScheduleId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel SMS schedule in Supabase.");
            throw;
        }
    }

    private async Task<SmsSettings?> LoadSettingsAsync(Supabase.Client client, Guid userId)
    {
        var response = await client.From<SmsSettingsRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Get();

        var record = response.Models.FirstOrDefault();
        var numbers = await client.From<SmsSenderRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Order(x => x.CreatedAt, PostgrestOrdering.Ascending)
            .Get();

        var settings = record is null
            ? new SmsSettings()
            : new SmsSettings
            {
                ProviderApiKey = record.ProviderApiKey ?? string.Empty,
                ProviderApiSecret = record.ProviderApiSecret ?? string.Empty,
                SenderId = record.SenderId ?? string.Empty,
                DefaultTemplate = record.DefaultTemplate ?? string.Empty
            };

        settings.SenderNumbers.Clear();
        foreach (var number in numbers.Models.Select(r => r.Number))
        {
            settings.SenderNumbers.Add(number);
        }

        var templatesResponse = await client.From<SmsTemplateRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Get();

        settings.Templates.Clear();
        foreach (var template in templatesResponse.Models.Select(t => t.Content))
        {
            settings.Templates.Add(template);
        }

        return settings;
    }

    private static SmsHistoryItem MapToHistoryItem(SmsHistoryRecord record)
    {
        IReadOnlyList<SmsAttachment> attachments;
        if (string.IsNullOrWhiteSpace(record.AttachmentsJson))
        {
            attachments = Array.Empty<SmsAttachment>();
        }
        else
        {
            attachments = JsonConvert.DeserializeObject<List<SmsAttachment>>(record.AttachmentsJson!)
                ?? new List<SmsAttachment>();
        }

        return new SmsHistoryItem(
            record.Recipient,
            record.Message,
            record.SentAt?.ToLocalTime() ?? DateTime.UtcNow,
            record.Status,
            record.SenderNumber ?? string.Empty,
            record.RecipientName ?? string.Empty,
            attachments);
    }

    private static SmsScheduleItem MapToScheduleItem(SmsScheduleRecord record)
    {
        BulkSmsRequest request;
        if (string.IsNullOrWhiteSpace(record.PayloadJson))
        {
            request = new BulkSmsRequest();
        }
        else
        {
            request = JsonConvert.DeserializeObject<BulkSmsRequest>(record.PayloadJson) ?? new BulkSmsRequest();
        }

        return new SmsScheduleItem(record.Id, record.ScheduledAt, request, record.IsCancelled);
    }

    private async Task CreateAuditLogAsync(Supabase.Client client, Guid userId, string action, string payload)
    {
        try
        {
            var record = new AuditLogRecord
            {
                Id = Guid.NewGuid(),
                ActorId = userId,
                Action = action,
                EntityType = "sms",
                EntityId = userId.ToString(),
                PayloadJson = payload,
                CreatedAt = DateTime.UtcNow
            };

            await client.From<AuditLogRecord>().Insert(record);
        }
        catch (PostgrestException ex)
        {
            _logger.LogWarning(ex, "Skipping SMS audit log write because the Supabase audit table is unavailable.");
        }
    }

    private async Task CreateIntegrationEventAsync(Supabase.Client client, string eventType, object payload)
    {
        try
        {
            var record = new IntegrationEventRecord
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                PayloadJson = JsonConvert.SerializeObject(payload),
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await client.From<IntegrationEventRecord>().Insert(record);
        }
        catch (PostgrestException ex)
        {
            _logger.LogWarning(ex, "Skipping integration event {EventType} because the Supabase edge pipeline is unavailable.", eventType);
        }
    }

    private Guid EnsureUserId(Supabase.Client client)
    {
        var rawId = client.Auth.CurrentUser?.Id;
        if (string.IsNullOrWhiteSpace(rawId) || !Guid.TryParse(rawId, out var parsed))
        {
            throw new InvalidOperationException("Supabase user id is required for SMS operations.");
        }

        return parsed;
    }
}

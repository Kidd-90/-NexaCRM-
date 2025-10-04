using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.CustomerCenter;
using NexaCRM.UI.Models.Supabase;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.Service.Supabase;

public sealed class SupabaseNoticeService : INoticeService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseNoticeService> _logger;

    public SupabaseNoticeService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseNoticeService> logger)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Notice>> GetNoticesAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<CustomerNoticeRecord>()
                .Order(x => x.IsPinned, PostgrestOrdering.Descending)
                .Order(x => x.PublishedAt, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<CustomerNoticeRecord>();
            return records.Select(MapToNotice).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load notices from Supabase.");
            throw;
        }
    }

    public async Task<Notice?> GetNoticeAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<CustomerNoticeRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            return record is null ? null : MapToNotice(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load notice {NoticeId} from Supabase.", id);
            throw;
        }
    }

    public async Task CreateNoticeAsync(Notice notice)
    {
        if (notice is null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = MapToRecord(notice);
            record.Id = default;

            await client.From<CustomerNoticeRecord>()
                .Insert(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notice in Supabase.");
            throw;
        }
    }

    public async Task UpdateNoticeAsync(Notice notice)
    {
        if (notice is null)
        {
            throw new ArgumentNullException(nameof(notice));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = MapToRecord(notice);

            await client.From<CustomerNoticeRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, notice.Id)
                .Update(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notice {NoticeId} in Supabase.", notice.Id);
            throw;
        }
    }

    public async Task DeleteNoticeAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            await client.From<CustomerNoticeRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notice {NoticeId} from Supabase.", id);
            throw;
        }
    }

    private static Notice MapToNotice(CustomerNoticeRecord record)
    {
        var publishedUtc = record.PublishedAt.Kind switch
        {
            DateTimeKind.Utc => record.PublishedAt,
            DateTimeKind.Local => record.PublishedAt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(record.PublishedAt, DateTimeKind.Utc)
        };

        return new Notice
        {
            Id = record.Id,
            Title = record.Title ?? string.Empty,
            Summary = record.Summary ?? string.Empty,
            Content = record.Content ?? string.Empty,
            Category = ParseCategory(record.Category),
            Importance = ParseImportance(record.Importance),
            PublishedAt = new DateTimeOffset(publishedUtc, TimeSpan.Zero),
            IsPinned = record.IsPinned,
            ReferenceUrl = record.ReferenceUrl,
            TenantId = record.TenantId
        };
    }

    private static CustomerNoticeRecord MapToRecord(Notice notice)
    {
        var publishedAt = notice.PublishedAt == default
            ? DateTime.UtcNow
            : notice.PublishedAt.UtcDateTime;

        return new CustomerNoticeRecord
        {
            Id = notice.Id,
            Title = notice.Title,
            Summary = string.IsNullOrWhiteSpace(notice.Summary) ? null : notice.Summary,
            Content = string.IsNullOrWhiteSpace(notice.Content) ? null : notice.Content,
            Category = SerializeCategory(notice.Category),
            Importance = SerializeImportance(notice.Importance),
            PublishedAt = DateTime.SpecifyKind(publishedAt, DateTimeKind.Utc),
            IsPinned = notice.IsPinned,
            ReferenceUrl = notice.ReferenceUrl,
            TenantId = notice.TenantId
        };
    }

    private static NoticeCategory ParseCategory(string? value)
    {
        return Enum.TryParse<NoticeCategory>(value, true, out var category)
            ? category
            : NoticeCategory.General;
    }

    private static NoticeImportance ParseImportance(string? value)
    {
        return Enum.TryParse<NoticeImportance>(value, true, out var importance)
            ? importance
            : NoticeImportance.Normal;
    }

    private static string SerializeCategory(NoticeCategory category) => category.ToString();

    private static string SerializeImportance(NoticeImportance importance) => importance.ToString();
}

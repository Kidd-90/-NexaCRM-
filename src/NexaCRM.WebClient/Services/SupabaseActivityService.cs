using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Enums;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using Task = System.Threading.Tasks.Task;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseActivityService : IActivityService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseActivityService> _logger;

    public SupabaseActivityService(SupabaseClientProvider clientProvider, ILogger<SupabaseActivityService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByContactIdAsync(int contactId)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<ActivityRecord>()
                .Filter(x => x.ContactId, PostgrestOperator.Equals, contactId)
                .Order(x => x.ActivityDate, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<ActivityRecord>();
            if (records.Count == 0)
            {
                return Array.Empty<Activity>();
            }

            return records.Select(MapToActivity).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load activities for contact {ContactId} from Supabase.", contactId);
            throw;
        }
    }

    public async Task AddActivityAsync(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = new ActivityRecord
            {
                Type = activity.Type.ToString(),
                Notes = activity.Content,
                ActivityDate = activity.Timestamp == default ? DateTime.UtcNow : activity.Timestamp,
                ContactId = activity.ContactId,
                CreatedByName = activity.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            await client.From<ActivityRecord>()
                .Insert(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create activity for contact {ContactId} in Supabase.", activity.ContactId);
            throw;
        }
    }

    private static Activity MapToActivity(ActivityRecord record)
    {
        return new Activity
        {
            Id = record.Id,
            ContactId = record.ContactId ?? 0,
            Type = ParseActivityType(record.Type),
            Content = record.Notes,
            Timestamp = record.ActivityDate,
            CreatedBy = string.IsNullOrWhiteSpace(record.CreatedByName)
                ? record.UserId?.ToString()
                : record.CreatedByName
        };
    }

    private static ActivityType ParseActivityType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ActivityType.Note;
        }

        return Enum.TryParse(value, ignoreCase: true, out ActivityType parsed)
            ? parsed
            : ActivityType.Note;
    }
}

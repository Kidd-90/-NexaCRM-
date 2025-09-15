using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class SmsService : ISmsService
{
    private readonly List<SmsScheduleItem> _schedules = new();

    public Task SendBulkSmsAsync(BulkSmsRequest request) =>
        Task.CompletedTask;

    public Task<IEnumerable<string>> GetSenderNumbersAsync() =>
        Task.FromResult<IEnumerable<string>>(new List<string>());

    public Task SaveSenderNumberAsync(string number) =>
        Task.CompletedTask;

    public Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync() =>
        Task.FromResult<IEnumerable<SmsHistoryItem>>(new List<SmsHistoryItem>());

    public Task ScheduleAsync(SmsScheduleItem schedule)
    {
        _schedules.Add(schedule);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<SmsScheduleItem>> GetUpcomingSchedulesAsync()
    {
        var now = DateTime.UtcNow;
        var upcoming = _schedules
            .Where(s => s.ScheduledAt > now && !s.IsCancelled)
            .OrderBy(s => s.ScheduledAt)
            .ToList();
        return Task.FromResult<IEnumerable<SmsScheduleItem>>(upcoming);
    }

    public Task CancelAsync(Guid id)
    {
        var item = _schedules.FirstOrDefault(s => s.Id == id);
        if (item != null)
        {
            var index = _schedules.IndexOf(item);
            _schedules[index] = item with { IsCancelled = true };
        }
        return Task.CompletedTask;
    }
}


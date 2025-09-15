using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Settings;
using NexaCRM.WebClient.Models.Sms;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ISmsService
{
    Task SendBulkAsync(IEnumerable<BulkSmsRequest> batches, IProgress<int>? progress = null);
    Task<IEnumerable<string>> GetSenderNumbersAsync();
    Task SaveSenderNumberAsync(string number);
    Task DeleteSenderNumberAsync(string number);
    Task<SmsSettings?> GetSettingsAsync();
    Task SaveSettingsAsync(SmsSettings settings);
    Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync();
    Task ScheduleAsync(SmsScheduleItem schedule);
    Task<IEnumerable<SmsScheduleItem>> GetUpcomingSchedulesAsync();
    Task CancelAsync(Guid id);
}


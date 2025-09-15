using NexaCRM.WebClient.Models.Sms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ISmsService
{
    Task SendBulkSmsAsync(BulkSmsRequest request);
    Task<IEnumerable<string>> GetSenderNumbersAsync();
    Task SaveSenderNumberAsync(string number);
    Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync();
    Task ScheduleAsync(SmsScheduleItem schedule);
    Task<IEnumerable<SmsScheduleItem>> GetUpcomingSchedulesAsync();
    Task CancelAsync(Guid id);
}


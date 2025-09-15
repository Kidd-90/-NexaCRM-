using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Models.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ISmsService
{
    Task SendBulkSmsAsync(BulkSmsRequest request);
    Task<IEnumerable<string>> GetSenderNumbersAsync();
    Task SaveSenderNumberAsync(string number);
    Task DeleteSenderNumberAsync(string number);
    Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync();
    Task ScheduleSmsAsync(SmsScheduleItem schedule);
    Task<SmsSettings> GetSettingsAsync();
    Task SaveSettingsAsync(SmsSettings settings);
}


using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class SmsService : ISmsService
{
    public Task SendBulkSmsAsync(BulkSmsRequest request) =>
        Task.CompletedTask;

    public Task<IEnumerable<string>> GetSenderNumbersAsync() =>
        Task.FromResult<IEnumerable<string>>(new List<string>());

    public Task SaveSenderNumberAsync(string number) =>
        Task.CompletedTask;

    public Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync() =>
        Task.FromResult<IEnumerable<SmsHistoryItem>>(new List<SmsHistoryItem>());

    public Task ScheduleSmsAsync(SmsScheduleItem schedule) =>
        Task.CompletedTask;
}


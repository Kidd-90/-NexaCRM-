using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class SmsService : ISmsService
{
    public async Task SendBulkAsync(IEnumerable<BulkSmsRequest> batches, IProgress<int>? progress = null)
    {
        var list = batches.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            await Task.Delay(10);
            progress?.Report((i + 1) * 100 / list.Count);
        }
    }

    public Task<IEnumerable<string>> GetSenderNumbersAsync() =>
        Task.FromResult<IEnumerable<string>>(new List<string>());

    public Task SaveSenderNumberAsync(string number) =>
        Task.CompletedTask;

    public Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync() =>
        Task.FromResult<IEnumerable<SmsHistoryItem>>(new List<SmsHistoryItem>());

    public Task ScheduleSmsAsync(SmsScheduleItem schedule) =>
        Task.CompletedTask;
}


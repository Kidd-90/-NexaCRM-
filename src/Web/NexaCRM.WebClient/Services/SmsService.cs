using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Services.Interfaces;
using System;
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
        Task.FromResult<IEnumerable<SmsHistoryItem>>(new List<SmsHistoryItem>
        {
            new("010-1234-5678", "Hello!", DateTime.UtcNow.AddDays(-1), "Sent"),
            new("010-2345-6789", "Reminder", DateTime.UtcNow.AddDays(-2), "Failed"),
            new("010-3456-7890", "Promotion", DateTime.UtcNow.AddDays(-3), "Sent"),
        });

    public Task ScheduleSmsAsync(SmsScheduleItem schedule) =>
        Task.CompletedTask;
}


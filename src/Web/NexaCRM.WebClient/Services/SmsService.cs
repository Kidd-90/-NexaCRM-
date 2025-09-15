using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Settings;
using NexaCRM.WebClient.Models.Sms;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public class SmsService : ISmsService
{
    private readonly List<string> _senderNumbers =
    [
        "+82 10-1234-5678",
        "+82 10-9876-5432"
    ];

    private readonly List<string> _templates =
    [
        "Welcome to NexaCRM",
        "Campaign follow-up",
        "Appointment reminder"
    ];

    private readonly List<SmsHistoryItem> _history = new();
    private readonly List<SmsScheduleItem> _schedules = new();
    private readonly SmsSettings _settings;

    public SmsService()
    {
        _settings = new SmsSettings
        {
            SenderNumbers = _senderNumbers,
            Templates = _templates,
            ProviderApiKey = "demo-api-key",
            ProviderApiSecret = "demo-api-secret",
            SenderId = "NEXACRM",
            DefaultTemplate = _templates.First()
        };
    }

    public async Task SendBulkAsync(IEnumerable<BulkSmsRequest> batches, IProgress<int>? progress = null)
    {
        var list = batches.ToList();
        if (list.Count == 0)
        {
            progress?.Report(100);
            return;
        }

        for (var i = 0; i < list.Count; i++)
        {
            await Task.Delay(10);
            var request = list[i];
            foreach (var recipient in request.Recipients)
            {
                _history.Add(new SmsHistoryItem(recipient, request.Message, DateTime.UtcNow, "Sent"));
            }
            progress?.Report((i + 1) * 100 / list.Count);
        }
    }

    public Task<IEnumerable<string>> GetSenderNumbersAsync() =>
        Task.FromResult<IEnumerable<string>>(_senderNumbers.ToList());

    public Task SaveSenderNumberAsync(string number)
    {
        if (!string.IsNullOrWhiteSpace(number) && !_senderNumbers.Contains(number, StringComparer.OrdinalIgnoreCase))
        {
            _senderNumbers.Add(number);
        }

        return Task.CompletedTask;
    }

    public Task DeleteSenderNumberAsync(string number)
    {
        _senderNumbers.RemoveAll(n => string.Equals(n, number, StringComparison.OrdinalIgnoreCase));
        return Task.CompletedTask;
    }

    public Task<SmsSettings?> GetSettingsAsync() =>
        Task.FromResult<SmsSettings?>(CloneSettings());

    public Task SaveSettingsAsync(SmsSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _settings.ProviderApiKey = settings.ProviderApiKey;
        _settings.ProviderApiSecret = settings.ProviderApiSecret;
        _settings.SenderId = settings.SenderId;
        _settings.DefaultTemplate = settings.DefaultTemplate;

        _senderNumbers.Clear();
        foreach (var number in settings.SenderNumbers.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            if (!_senderNumbers.Contains(number, StringComparer.OrdinalIgnoreCase))
            {
                _senderNumbers.Add(number);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<SmsHistoryItem>> GetHistoryAsync() =>
        Task.FromResult<IEnumerable<SmsHistoryItem>>(_history
            .OrderByDescending(item => item.SentAt)
            .ToList());

    public Task ScheduleAsync(SmsScheduleItem schedule)
    {
        _schedules.RemoveAll(s => s.Id == schedule.Id);
        _schedules.Add(schedule);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<SmsScheduleItem>> GetUpcomingSchedulesAsync() =>
        Task.FromResult<IEnumerable<SmsScheduleItem>>(_schedules
            .Where(s => !s.IsCancelled && s.ScheduledAt >= DateTime.UtcNow.AddMinutes(-1))
            .OrderBy(s => s.ScheduledAt)
            .ToList());

    public Task CancelAsync(Guid id)
    {
        var index = _schedules.FindIndex(s => s.Id == id);
        if (index >= 0)
        {
            _schedules[index] = _schedules[index] with { IsCancelled = true };
        }

        return Task.CompletedTask;
    }

    private SmsSettings CloneSettings() => new()
    {
        ProviderApiKey = _settings.ProviderApiKey,
        ProviderApiSecret = _settings.ProviderApiSecret,
        SenderId = _settings.SenderId,
        DefaultTemplate = _settings.DefaultTemplate,
        SenderNumbers = new List<string>(_senderNumbers),
        Templates = new List<string>(_templates)
    };
}


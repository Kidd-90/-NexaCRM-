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

        SeedHistory();
    }

    public async Task SendBulkAsync(IEnumerable<BulkSmsRequest> batches, IProgress<int>? progress = null)
    {
        var list = batches.ToList();
        if (list.Count == 0)
        {
            progress?.Report(100);
            return;
        }

        var defaultSender = _senderNumbers.FirstOrDefault() ?? _settings.SenderId;

        for (var i = 0; i < list.Count; i++)
        {
            await Task.Delay(10);
            var request = list[i];
            foreach (var recipient in request.Recipients)
            {
                _history.Add(new SmsHistoryItem(
                    recipient,
                    request.Message,
                    DateTime.UtcNow,
                    "Sent",
                    defaultSender,
                    string.Empty,
                    Array.Empty<SmsAttachment>()));
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

    private void SeedHistory()
    {
        if (_history.Count > 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        _history.AddRange(new[]
        {
            new SmsHistoryItem(
                "010-1234-5678",
                "다음 주 미팅 관련 자료 전달드립니다.",
                now.AddHours(-3),
                "Sent",
                "+82 10-1234-5678",
                "홍길동",
                new List<SmsAttachment>
                {
                    new("미팅자료.pdf", 248_576, "application/pdf"),
                    new("제품소개서.pptx", 5_242_880, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
                }),
            new SmsHistoryItem(
                "010-9876-5432",
                "상담 예약이 확인되었습니다. 일정은 5월 28일 오후 2시입니다.",
                now.AddDays(-1).AddHours(-2),
                "Sent",
                "+82 10-9876-5432",
                "김민지",
                Array.Empty<SmsAttachment>()),
            new SmsHistoryItem(
                "010-5555-4444",
                "요청하신 보험 설계안을 첨부했습니다. 확인 부탁드립니다.",
                now.AddDays(-2).AddHours(-5),
                "Sent",
                "+82 10-1234-5678",
                "박서준",
                new List<SmsAttachment>
                {
                    new("설계안.pdf", 1_048_576, "application/pdf"),
                    new("상품비교.xlsx", 384_000, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                }),
            new SmsHistoryItem(
                "010-3333-2222",
                "파일 용량 제한으로 전송에 실패했습니다. 다시 시도해주세요.",
                now.AddDays(-3).AddHours(-1),
                "Failed",
                "+82 10-9876-5432",
                "최은우",
                new List<SmsAttachment>
                {
                    new("프로모션영상.mp4", 45_056_000, "video/mp4")
                })
        });
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


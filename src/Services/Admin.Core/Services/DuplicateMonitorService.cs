using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services
{
    public class DuplicateMonitorService : IDuplicateMonitorService, IDisposable
    {
        private readonly IDuplicateService _duplicateService;
        private readonly IDedupeConfigService _config;
        private readonly INotificationFeedService _feed;
        private Timer? _timer;
        private int _lastCount;

        public DuplicateMonitorService(IDuplicateService duplicateService, IDedupeConfigService config, INotificationFeedService feed)
        {
            _duplicateService = duplicateService;
            _config = config;
            _feed = feed;
            _config.Changed += OnConfigChanged;
        }

        public Task StartAsync()
        {
            RestartTimer();
            return Task.CompletedTask;
        }

        private async void OnTick(object? state)
        {
            try
            {
                if (!_config.Enabled) return;
                var groups = await _duplicateService.FindDuplicatesAsync(_config.Days, _config.IncludeFuzzy);
                var count = groups.Count;
                if (count > 0 && (_config.NotifyOnSameCount || count != _lastCount))
                {
                    await _feed.AddAsync(new NotificationFeedItem
                    {
                        Title = "중복 DB 감지 결과",
                        Message = $"최근 {_config.Days}일 내 중복 {count}건 발견",
                        Type = "warning",
                        TimestampUtc = DateTime.UtcNow,
                        IsRead = false
                    });
                    _lastCount = count;
                }
            }
            catch
            {
                // swallow for mock environment
            }
        }

        private void OnConfigChanged()
        {
            _lastCount = 0; // force next tick to notify again with new settings
            RestartTimer();
        }

        private void RestartTimer()
        {
            var due = TimeSpan.FromMinutes(2);
            var period = TimeSpan.FromMinutes(Math.Max(1, _config.MonitorIntervalMinutes));
            _timer?.Dispose();
            _timer = new Timer(OnTick, null, due, period);
        }

        public Task RunOnceAsync()
        {
            OnTick(null);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _config.Changed -= OnConfigChanged;
            _timer?.Dispose();
        }
    }
}

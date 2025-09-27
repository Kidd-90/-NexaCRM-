using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;

namespace NexaCRM.Services.Admin;

public sealed class DuplicateMonitorService : IDuplicateMonitorService, IAsyncDisposable
{
    private readonly IDuplicateService _duplicateService;
    private readonly IDedupeConfigService _config;
    private readonly INotificationFeedService _feed;
    private readonly ILogger<DuplicateMonitorService> _logger;
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;
    private int _lastCount;

    public DuplicateMonitorService(
        IDuplicateService duplicateService,
        IDedupeConfigService config,
        INotificationFeedService feed,
        ILogger<DuplicateMonitorService> logger)
    {
        _duplicateService = duplicateService;
        _config = config;
        _feed = feed;
        _logger = logger;
        _config.Changed += OnConfigChanged;
    }

    public Task StartAsync()
    {
        RestartLoop();
        return Task.CompletedTask;
    }

    public Task RunOnceAsync() => EvaluateOnceAsync(CancellationToken.None);

    private void OnConfigChanged()
    {
        _lastCount = 0;
        RestartLoop();
    }

    private void RestartLoop()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        if (!_config.Enabled)
        {
            _monitorTask = null;
            return;
        }

        _cts = new CancellationTokenSource();
        _monitorTask = MonitorAsync(_cts.Token);
    }

    private async Task MonitorAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var delay = TimeSpan.FromMinutes(Math.Max(1, _config.MonitorIntervalMinutes));
                await Task.Delay(delay, token);
                await EvaluateOnceAsync(token);
            }
            catch (OperationCanceledException)
            {
                // expected when the token is canceled
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Duplicate monitor loop failed.");
            }
        }
    }

    private async Task EvaluateOnceAsync(CancellationToken token)
    {
        if (!_config.Enabled)
        {
            return;
        }

        var groups = await _duplicateService.FindDuplicatesAsync(_config.Days, _config.IncludeFuzzy);
        var count = groups.Count;
        if (count <= 0)
        {
            return;
        }

        if (!_config.NotifyOnSameCount && count == _lastCount)
        {
            return;
        }

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

    public async ValueTask DisposeAsync()
    {
        _config.Changed -= OnConfigChanged;
        if (_cts is null)
        {
            return;
        }

        _cts.Cancel();
        try
        {
            if (_monitorTask is not null)
            {
                await _monitorTask.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore cancellation during shutdown
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }
    }
}

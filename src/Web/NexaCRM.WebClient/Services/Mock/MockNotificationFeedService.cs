using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services.Mock;

public class MockNotificationFeedService : INotificationFeedService
{
    private static readonly List<NotificationFeedItem> Store = new()
    {
        new NotificationFeedItem { Title = "새 작업이 배정되었습니다", Message = "담당: 홍길동 | 마감: 오늘", Type = "info", IsRead = false, TimestampUtc = DateTime.UtcNow.AddMinutes(-10) },
        new NotificationFeedItem { Title = "거래 단계 변경", Message = "ABC 유통 → 협상 진행", Type = "success", IsRead = false, TimestampUtc = DateTime.UtcNow.AddHours(-1) },
        new NotificationFeedItem { Title = "시스템 점검 안내", Message = "오늘 23:00 ~ 23:30", Type = "warning", IsRead = true, TimestampUtc = DateTime.UtcNow.AddDays(-1) },
    };

    public event Action<int>? UnreadCountChanged;

    public event Action<IReadOnlyList<NotificationFeedItem>>? FeedUpdated;

    public Task<IReadOnlyList<NotificationFeedItem>> GetAsync()
    {
        var snapshot = Store
            .OrderByDescending(x => x.TimestampUtc)
            .ToList();

        return Task.FromResult((IReadOnlyList<NotificationFeedItem>)snapshot);
    }

    public Task<int> GetUnreadCountAsync()
        => Task.FromResult(Store.Count(x => !x.IsRead));

    public Task MarkAllReadAsync()
    {
        foreach (var item in Store)
        {
            item.IsRead = true;
        }

        NotifyObservers();
        return Task.CompletedTask;
    }

    public Task MarkAsReadAsync(Guid id)
    {
        var item = Store.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            item.IsRead = true;
            NotifyObservers();
        }

        return Task.CompletedTask;
    }

    public Task AddAsync(NotificationFeedItem item)
    {
        item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
        Store.Add(item);
        NotifyObservers();
        return Task.CompletedTask;
    }

    private void NotifyObservers()
    {
        var unread = Store.Count(x => !x.IsRead);
        var snapshot = (IReadOnlyList<NotificationFeedItem>)Store
            .OrderByDescending(x => x.TimestampUtc)
            .ToList();

        UnreadCountChanged?.Invoke(unread);
        FeedUpdated?.Invoke(snapshot);
    }
}


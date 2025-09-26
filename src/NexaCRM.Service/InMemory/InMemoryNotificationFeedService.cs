using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;

namespace NexaCRM.Service.InMemory;

/// <summary>
/// Thread-safe in-memory notification feed for the Blazor Server host.
/// </summary>
public sealed class InMemoryNotificationFeedService : INotificationFeedService
{
    private readonly object _syncRoot = new();
    private readonly List<NotificationFeedItem> _items = new();

    public event Action<int>? UnreadCountChanged;
    public event Action<IReadOnlyList<NotificationFeedItem>>? FeedUpdated;

    public Task<IReadOnlyList<NotificationFeedItem>> GetAsync()
    {
        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyList<NotificationFeedItem>>(_items.Select(Clone).ToList());
        }
    }

    public Task<int> GetUnreadCountAsync()
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_items.Count(item => !item.IsRead));
        }
    }

    public Task MarkAllReadAsync()
    {
        lock (_syncRoot)
        {
            foreach (var item in _items)
            {
                item.IsRead = true;
            }

            RaiseEvents();
            return Task.CompletedTask;
        }
    }

    public Task MarkAsReadAsync(Guid id)
    {
        lock (_syncRoot)
        {
            var target = _items.FirstOrDefault(item => item.Id == id);
            if (target is not null)
            {
                target.IsRead = true;
            }

            RaiseEvents();
            return Task.CompletedTask;
        }
    }

    public Task AddAsync(NotificationFeedItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_syncRoot)
        {
            var entry = Clone(item);
            entry.Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id;
            entry.TimestampUtc = entry.TimestampUtc == default ? DateTime.UtcNow : entry.TimestampUtc;

            _items.Insert(0, entry);
            _items.Sort((left, right) => DateTime.Compare(right.TimestampUtc, left.TimestampUtc));

            RaiseEvents();
            return Task.CompletedTask;
        }
    }

    private void RaiseEvents()
    {
        var snapshot = _items.Select(Clone).ToList();
        var unread = snapshot.Count(item => !item.IsRead);

        FeedUpdated?.Invoke(snapshot);
        UnreadCountChanged?.Invoke(unread);
    }

    private static NotificationFeedItem Clone(NotificationFeedItem source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        Message = source.Message,
        TimestampUtc = source.TimestampUtc,
        IsRead = source.IsRead,
        Type = source.Type
    };
}

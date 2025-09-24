using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface INotificationFeedService
{
    Task<IReadOnlyList<NotificationFeedItem>> GetAsync();
    Task<int> GetUnreadCountAsync();
    Task MarkAllReadAsync();
    Task MarkAsReadAsync(Guid id);
    Task AddAsync(NotificationFeedItem item);

    event Action<int>? UnreadCountChanged;
    event Action<IReadOnlyList<NotificationFeedItem>>? FeedUpdated;
}

public sealed class NotificationFeedItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public string Type { get; set; } = "info";
}


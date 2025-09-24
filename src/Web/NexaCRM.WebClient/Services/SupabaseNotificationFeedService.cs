using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Services.Interfaces;
using NexaCRM.WebClient.Models.Supabase;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using RealtimeEventType = Supabase.Realtime.Constants.EventType;
using RealtimeListenType = Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType;
using Supabase.Realtime.Interfaces;
using Supabase.Realtime.PostgresChanges;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseNotificationFeedService : INotificationFeedService, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseNotificationFeedService> _logger;
    private readonly Dictionary<Guid, NotificationFeedItem> _cache = new();
    private readonly object _syncRoot = new();
    private readonly SemaphoreSlim _subscriptionLock = new(1, 1);
    private bool _subscriptionInitialized;
    private IRealtimeChannel? _realtimeChannel;
    private IRealtimeChannel.PostgresChangesHandler? _changeHandler;
    private Guid? _userId;

    public event Action<int>? UnreadCountChanged;
    public event Action<IReadOnlyList<NotificationFeedItem>>? FeedUpdated;

    public SupabaseNotificationFeedService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseNotificationFeedService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<NotificationFeedItem>> GetAsync()
    {
        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var response = await client.From<NotificationFeedRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<NotificationFeedRecord>();
            var items = records.Select(MapToItem).ToList();

            lock (_syncRoot)
            {
                _cache.Clear();
                foreach (var item in items)
                {
                    _cache[item.Id] = item;
                }
            }

            NotifyUnreadCount();
            NotifyFeedUpdated(items);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load notification feed from Supabase.");
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await EnsureRealtimeSubscriptionAsync();

        lock (_syncRoot)
        {
            if (_cache.Count > 0)
            {
                return _cache.Values.Count(item => !item.IsRead);
            }
        }

        var client = await _clientProvider.GetClientAsync();
        var userId = EnsureUserId(client);

        var response = await client.From<NotificationFeedRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Filter(x => x.IsRead, PostgrestOperator.Equals, false)
            .Get();

        return response.Models.Count;
    }

    public async Task MarkAllReadAsync()
    {
        await EnsureRealtimeSubscriptionAsync();

        List<Guid> unreadIds;
        lock (_syncRoot)
        {
            unreadIds = _cache.Values
                .Where(item => !item.IsRead)
                .Select(item => item.Id)
                .ToList();
        }

        foreach (var id in unreadIds)
        {
            await MarkAsReadAsync(id);
        }
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        await EnsureRealtimeSubscriptionAsync();
        var client = await _clientProvider.GetClientAsync();
        var userId = EnsureUserId(client);

        NotificationFeedItem? item;
        lock (_syncRoot)
        {
            _cache.TryGetValue(id, out item);
        }

        if (item is null)
        {
            var response = await client.From<NotificationFeedRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();
            var record = response.Models.FirstOrDefault();
            if (record is null)
            {
                return;
            }

            item = MapToItem(record);
        }

        if (item.IsRead)
        {
            return;
        }

        var updatedItem = new NotificationFeedItem
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.Message,
            TimestampUtc = item.TimestampUtc,
            IsRead = true,
            Type = item.Type
        };
        var updatedRecord = MapToRecord(updatedItem, userId);
        await client.From<NotificationFeedRecord>()
            .Filter(x => x.Id, PostgrestOperator.Equals, id)
            .Update(updatedRecord);

        StoreNotification(updatedItem);
    }

    public async Task AddAsync(NotificationFeedItem item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        await EnsureRealtimeSubscriptionAsync();
        var client = await _clientProvider.GetClientAsync();
        var userId = EnsureUserId(client);
        var timestamp = item.TimestampUtc == default ? DateTime.UtcNow : item.TimestampUtc;
        var timestampedItem = new NotificationFeedItem
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.Message,
            TimestampUtc = timestamp,
            IsRead = item.IsRead,
            Type = item.Type
        };

        var record = MapToRecord(timestampedItem, userId);
        var response = await client.From<NotificationFeedRecord>().Insert(record);
        var inserted = response.Models.FirstOrDefault();
        if (inserted is not null)
        {
            StoreNotification(inserted);
        }
    }

    private async Task EnsureRealtimeSubscriptionAsync()
    {
        if (_subscriptionInitialized)
        {
            return;
        }

        await _subscriptionLock.WaitAsync();
        try
        {
            if (_subscriptionInitialized)
            {
                return;
            }

            var client = await _clientProvider.GetClientAsync();
            EnsureUserId(client);
            _changeHandler ??= HandleRealtimeChange;
            _realtimeChannel = await client.From<NotificationFeedRecord>()
                .On(RealtimeListenType.All, _changeHandler);

            _subscriptionInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to subscribe to Supabase notification realtime channel.");
        }
        finally
        {
            _subscriptionLock.Release();
        }
    }

    private void HandleRealtimeChange(IRealtimeChannel sender, PostgresChangesResponse change)
    {
        try
        {
            var eventType = change.Payload?.Data?.Type;
            if (eventType is null)
            {
                return;
            }

            var record = change.Model<NotificationFeedRecord>();
            var targetUserId = record?.UserId ?? change.OldModel<NotificationFeedRecord>()?.UserId;
            if (_userId.HasValue && targetUserId.HasValue && targetUserId.Value != _userId.Value)
            {
                return;
            }

            switch (eventType.Value)
            {
                case RealtimeEventType.Insert:
                case RealtimeEventType.Update:
                    if (record is null)
                    {
                        return;
                    }

                    StoreNotification(record);
                    break;
                case RealtimeEventType.Delete:
                    HandleNotificationDeleted(change);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process notification realtime payload.");
        }
    }

    private void HandleNotificationDeleted(PostgresChangesResponse change)
    {
        var record = change.OldModel<NotificationFeedRecord>();
        Guid? notificationId = record?.Id;

        if (!notificationId.HasValue)
        {
            return;
        }

        var shouldNotify = false;
        int currentUnread = 0;
        lock (_syncRoot)
        {
            var previousUnread = _cache.Values.Count(item => !item.IsRead);
            _cache.Remove(notificationId.Value);
            currentUnread = _cache.Values.Count(item => !item.IsRead);
            shouldNotify = previousUnread != currentUnread;
        }

        if (shouldNotify)
        {
            NotifyUnreadCount(currentUnread);
        }

        NotifyFeedUpdated();
    }

    private void StoreNotification(NotificationFeedRecord record) => StoreNotification(MapToItem(record));

    private void StoreNotification(NotificationFeedItem item)
    {
        var shouldNotify = false;
        int currentUnread = 0;
        lock (_syncRoot)
        {
            var previousUnread = _cache.Values.Count(existing => !existing.IsRead);
            _cache[item.Id] = item;
            currentUnread = _cache.Values.Count(existing => !existing.IsRead);
            shouldNotify = previousUnread != currentUnread;
        }

        if (shouldNotify)
        {
            NotifyUnreadCount(currentUnread);
        }

        NotifyFeedUpdated();
    }

    private NotificationFeedRecord MapToRecord(NotificationFeedItem item, Guid userId)
    {
        var timestamp = item.TimestampUtc == default ? DateTime.UtcNow : item.TimestampUtc;
        return new NotificationFeedRecord
        {
            Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
            UserId = userId,
            Title = item.Title,
            Message = item.Message,
            Type = item.Type,
            IsRead = item.IsRead,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };
    }

    private static NotificationFeedItem MapToItem(NotificationFeedRecord record)
    {
        return new NotificationFeedItem
        {
            Id = record.Id,
            Title = record.Title,
            Message = record.Message ?? string.Empty,
            TimestampUtc = record.CreatedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            IsRead = record.IsRead,
            Type = record.Type ?? "info"
        };
    }

    private void NotifyFeedUpdated(IReadOnlyList<NotificationFeedItem>? snapshot = null)
    {
        var handler = FeedUpdated;
        if (handler is null)
        {
            return;
        }

        IReadOnlyList<NotificationFeedItem> items;
        if (snapshot is not null)
        {
            items = snapshot;
        }
        else
        {
            lock (_syncRoot)
            {
                items = _cache.Values
                    .OrderByDescending(item => item.TimestampUtc)
                    .ToList();
            }
        }

        handler(items);
    }

    private void NotifyUnreadCount(int? countOverride = null)
    {
        int count;
        if (countOverride.HasValue)
        {
            count = countOverride.Value;
        }
        else
        {
            lock (_syncRoot)
            {
                count = _cache.Values.Count(item => !item.IsRead);
            }
        }

        UnreadCountChanged?.Invoke(count);
    }

    private Guid EnsureUserId(Supabase.Client client)
    {
        var rawId = client.Auth.CurrentUser?.Id;
        if (string.IsNullOrWhiteSpace(rawId) || !Guid.TryParse(rawId, out var parsed))
        {
            throw new InvalidOperationException("Supabase user id is required for notifications.");
        }

        if (!_userId.HasValue || _userId.Value != parsed)
        {
            _userId = parsed;
        }

        return _userId.Value;
    }

    public ValueTask DisposeAsync()
    {
        if (_realtimeChannel is not null && _changeHandler is not null)
        {
            try
            {
                _realtimeChannel.RemovePostgresChangeHandler(RealtimeListenType.All, _changeHandler);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to remove notification realtime handler.");
            }
        }

        _realtimeChannel = null;
        _changeHandler = null;
        _subscriptionInitialized = false;

        return ValueTask.CompletedTask;
    }
}

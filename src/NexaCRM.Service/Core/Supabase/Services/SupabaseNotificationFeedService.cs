using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.UI.Models.Supabase;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using RealtimeEventType = global::Supabase.Realtime.Constants.EventType;
using RealtimeListenType = global::Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType;
using global::Supabase.Realtime.Interfaces;
using global::Supabase.Realtime.PostgresChanges;

namespace NexaCRM.Service.Supabase;

public sealed class SupabaseNotificationFeedService : INotificationFeedService, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly AuthenticationStateProvider _authStateProvider;
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
        AuthenticationStateProvider authStateProvider,
        ILogger<SupabaseNotificationFeedService> logger)
    {
        _clientProvider = clientProvider;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<NotificationFeedItem>> GetAsync()
    {
        try
        {
            _logger.LogInformation("[GetAsync] Starting to load notification feed...");
            
            await EnsureRealtimeSubscriptionAsync();
            _logger.LogInformation("[GetAsync] Realtime subscription ensured.");
            
            // Get user ID from authentication state (claims)
            var (hasUserId, userId) = await TryEnsureUserIdAsync();
            if (!hasUserId)
            {
                _logger.LogWarning("[GetAsync] ❌ No authenticated user available when loading notification feed; returning empty feed.");
                return new List<NotificationFeedItem>();
            }
            
            _logger.LogInformation("[GetAsync] ✅ User ID obtained from claims: {UserId}", userId);
            
            var client = await _clientProvider.GetClientAsync();
            _logger.LogInformation("[GetAsync] Supabase client obtained.");

            _logger.LogInformation("[GetAsync] 🔍 Executing query: SELECT * FROM notification_feed WHERE user_id = '{UserId}' ORDER BY created_at DESC", userId);
            var response = await client.From<NotificationFeedRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId.ToString())
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Get();

            _logger.LogInformation("[GetAsync] ✅ Query executed successfully. Response Models Count: {Count}", response.Models?.Count ?? 0);
            
            // HTTP 응답 상태 코드만 로깅 (Content는 이미 dispose되었을 수 있음)
            if (response.ResponseMessage != null)
            {
                _logger.LogInformation("[GetAsync] HTTP Status: {StatusCode}", response.ResponseMessage.StatusCode);
            }
            
            var records = response.Models ?? new List<NotificationFeedRecord>();
            _logger.LogInformation("[GetAsync] Retrieved {Count} records from database.", records.Count);
            
            if (records.Count > 0)
            {
                var firstRecord = records.First();
                _logger.LogInformation("[GetAsync] First record - Id: {Id}, Title: {Title}, Type: {Type}, IsRead: {IsRead}",
                    firstRecord.Id, firstRecord.Title, firstRecord.Type, firstRecord.IsRead);
            }
            
            var items = records.Select(MapToItem).ToList();
            _logger.LogInformation("[GetAsync] Mapped {Count} records to NotificationFeedItems.", items.Count);

            lock (_syncRoot)
            {
                _cache.Clear();
                foreach (var item in items)
                {
                    _cache[item.Id] = item;
                }
            }
            
            _logger.LogInformation("[GetAsync] Cache updated with {Count} items.", items.Count);

            NotifyUnreadCount();
            NotifyFeedUpdated(items);
            
            _logger.LogInformation("[GetAsync] Successfully loaded notification feed with {Count} items.", items.Count);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetAsync] Failed to load notification feed from Supabase. Error: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("[GetAsync] Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
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
        if (!TryEnsureUserId(client, out var userId))
        {
            _logger.LogDebug("No authenticated Supabase user available when getting unread count; returning 0.");
            return 0;
        }

        var response = await client.From<NotificationFeedRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId.ToString())
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
        if (!TryEnsureUserId(client, out var userId))
        {
            _logger.LogDebug("No authenticated Supabase user available when marking notification as read; skipping.");
            return;
        }

        NotificationFeedItem? item;
        lock (_syncRoot)
        {
            _cache.TryGetValue(id, out item);
        }

        if (item is null)
        {
            var response = await client.From<NotificationFeedRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id.ToString())
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
            .Filter(x => x.Id, PostgrestOperator.Equals, id.ToString())
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
        if (!TryEnsureUserId(client, out var userId))
        {
            _logger.LogDebug("No authenticated Supabase user available when adding notification; skipping insert.");
            return;
        }
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
            if (!TryEnsureUserId(client, out _))
            {
                _logger.LogDebug("Realtime subscription not initialized because no authenticated Supabase user is available.");
                return;
            }
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
            // If we don't have a current user id set, ignore realtime events.
            if (!_userId.HasValue)
            {
                return;
            }

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

    private Guid EnsureUserId(global::Supabase.Client client)
    {
        // Backwards-compatible helper that throws when no user id is available.
        if (!TryEnsureUserId(client, out var id))
        {
            throw new InvalidOperationException("Supabase user id is required for notifications.");
        }

        return id;
    }

    private async Task<(bool success, Guid userId)> TryEnsureUserIdAsync()
    {
        try
        {
            // Try to get user ID from AuthenticationStateProvider
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Try to get user ID from NameIdentifier claim
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsed))
                {
                    _logger.LogInformation("[TryEnsureUserIdAsync] ✅ User ID from claims: {UserId}", parsed);
                    
                    if (!_userId.HasValue || _userId.Value != parsed)
                    {
                        _userId = parsed;
                    }
                    
                    return (true, _userId.Value);
                }
            }
            
            _logger.LogWarning("[TryEnsureUserIdAsync] ❌ No authenticated user found in claims");
            return (false, Guid.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TryEnsureUserIdAsync] ❌ Error getting user ID from authentication state");
            return (false, Guid.Empty);
        }
    }
    
    private bool TryEnsureUserId(global::Supabase.Client? client, out Guid userId)
    {
        // Legacy synchronous version - fallback to client.Auth
        userId = Guid.Empty;
        var rawId = client?.Auth?.CurrentUser?.Id;
        if (string.IsNullOrWhiteSpace(rawId) || !Guid.TryParse(rawId, out var parsed))
        {
            return false;
        }

        if (!_userId.HasValue || _userId.Value != parsed)
        {
            _userId = parsed;
        }

        userId = _userId.Value;
        return true;
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

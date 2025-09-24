using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using RealtimeEventType = Supabase.Realtime.Constants.EventType;
using RealtimeListenType = Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType;
using Supabase.Realtime.Interfaces;
using Supabase.Realtime.PostgresChanges;
using SupportTicket = NexaCRM.WebClient.Models.SupportTicket;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseSupportTicketService : ISupportTicketService, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseSupportTicketService> _logger;
    private readonly Dictionary<int, SupportTicket> _cache = new();
    private readonly object _syncRoot = new();
    private readonly SemaphoreSlim _subscriptionLock = new(1, 1);
    private bool _subscriptionInitialized;
    private IRealtimeChannel? _realtimeChannel;
    private IRealtimeChannel.PostgresChangesHandler? _changeHandler;

    public event Action<SupportTicket>? TicketUpserted;
    public event Action<int>? TicketDeleted;
    public event Action<int>? LiveTicketCountChanged;

    public SupabaseSupportTicketService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseSupportTicketService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<SupportTicket>> GetTicketsAsync()
    {
        try
        {
            await EnsureRealtimeSubscriptionAsync();

            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<SupportTicketRecord>()
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<SupportTicketRecord>();
            var tickets = records.Select(MapToTicket).ToList();

            lock (_syncRoot)
            {
                _cache.Clear();
                foreach (var ticket in tickets)
                {
                    _cache[ticket.Id] = ticket;
                }
            }

            NotifyLiveTicketCount();
            return tickets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load support tickets from Supabase.");
            throw;
        }
    }

    public async Task<SupportTicket?> GetTicketByIdAsync(int id)
    {
        try
        {
            await EnsureRealtimeSubscriptionAsync();

            lock (_syncRoot)
            {
                if (_cache.TryGetValue(id, out var cached))
                {
                    return cached;
                }
            }

            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<SupportTicketRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            if (record is null)
            {
                return null;
            }

            var ticket = MapToTicket(record);
            lock (_syncRoot)
            {
                _cache[ticket.Id] = ticket;
            }

            NotifyLiveTicketCount();
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load support ticket {TicketId} from Supabase.", id);
            throw;
        }
    }

    public async Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync()
    {
        await EnsureRealtimeSubscriptionAsync();

        List<SupportTicket> snapshot;
        lock (_syncRoot)
        {
            if (_cache.Count == 0)
            {
                snapshot = new List<SupportTicket>();
            }
            else
            {
                snapshot = _cache.Values.ToList();
            }
        }

        if (snapshot.Count == 0)
        {
            snapshot = (await GetTicketsAsync()).ToList();
        }

        return snapshot.Where(ticket =>
            ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.InProgress).ToList();
    }

    public async Task CreateTicketAsync(SupportTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            var currentUserId = client.Auth.CurrentUser?.Id;
            var createdAt = ticket.CreatedAt == default ? DateTime.UtcNow : ticket.CreatedAt;
            var agentId = ticket.AgentId ?? ParseGuid(ticket.AgentName);

            var record = new SupportTicketRecord
            {
                Subject = ticket.Subject,
                Description = ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                CustomerId = null,
                CustomerName = ticket.CustomerName,
                AgentName = ticket.AgentName,
                AgentId = agentId,
                Category = ticket.Category,
                CreatedBy = ParseGuid(currentUserId),
                CreatedAt = createdAt,
                UpdatedAt = createdAt,
                TenantUnitId = null
            };

            var response = await client.From<SupportTicketRecord>().Insert(record);
            var inserted = response.Models.FirstOrDefault();
            if (inserted is not null)
            {
                var created = MapToTicket(inserted);
                StoreTicket(created);
                TicketUpserted?.Invoke(created);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create support ticket in Supabase.");
            throw;
        }
    }

    public async Task UpdateTicketAsync(SupportTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            var agentId = ticket.AgentId ?? ParseGuid(ticket.AgentName);

            var record = new SupportTicketRecord
            {
                Id = ticket.Id,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority.ToString(),
                CustomerName = ticket.CustomerName,
                AgentName = ticket.AgentName,
                AgentId = agentId,
                Category = ticket.Category,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await client.From<SupportTicketRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, ticket.Id)
                .Update(record);

            var updatedRecord = response.Models.FirstOrDefault() ?? record;
            var updated = MapToTicket(updatedRecord);
            StoreTicket(updated);
            TicketUpserted?.Invoke(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update support ticket {TicketId} in Supabase.", ticket.Id);
            throw;
        }
    }

    public async Task DeleteTicketAsync(int id)
    {
        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            await client.From<SupportTicketRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();

            var removed = false;
            lock (_syncRoot)
            {
                removed = _cache.Remove(id);
            }

            if (removed)
            {
                TicketDeleted?.Invoke(id);
                NotifyLiveTicketCount();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete support ticket {TicketId} from Supabase.", id);
            throw;
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
            _changeHandler ??= HandleRealtimeChange;
            _realtimeChannel = await client.From<SupportTicketRecord>()
                .On(RealtimeListenType.All, _changeHandler);

            _subscriptionInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to subscribe to Supabase support ticket realtime channel.");
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

            switch (eventType.Value)
            {
                case RealtimeEventType.Insert:
                case RealtimeEventType.Update:
                    var record = change.Model<SupportTicketRecord>();
                    if (record is null)
                    {
                        return;
                    }

                    var ticket = MapToTicket(record);
                    StoreTicket(ticket);
                    TicketUpserted?.Invoke(ticket);
                    break;

                case RealtimeEventType.Delete:
                    HandleTicketDeleted(change);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process support ticket realtime payload.");
        }
    }

    private void HandleTicketDeleted(PostgresChangesResponse change)
    {
        int? ticketId = change.OldModel<SupportTicketRecord>()?.Id;
        if (ticketId is null)
        {
            var firstId = change.Payload?.Data?.Ids?.FirstOrDefault();
            if (firstId.HasValue)
            {
                ticketId = firstId.Value;
            }
        }

        if (ticketId is null)
        {
            return;
        }

        var removed = false;
        lock (_syncRoot)
        {
            removed = _cache.Remove(ticketId.Value);
        }

        if (removed)
        {
            TicketDeleted?.Invoke(ticketId.Value);
            NotifyLiveTicketCount();
        }
    }

    private void StoreTicket(SupportTicket ticket)
    {
        lock (_syncRoot)
        {
            _cache[ticket.Id] = ticket;
        }

        NotifyLiveTicketCount();
    }

    private void NotifyLiveTicketCount()
    {
        int liveCount;
        lock (_syncRoot)
        {
            liveCount = _cache.Values.Count(t =>
                t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);
        }

        LiveTicketCountChanged?.Invoke(liveCount);
    }

    private static SupportTicket MapToTicket(SupportTicketRecord record)
    {
        return new SupportTicket
        {
            Id = record.Id,
            Subject = record.Subject,
            Description = record.Description,
            Status = ParseStatus(record.Status),
            Priority = ParsePriority(record.Priority),
            CustomerName = record.CustomerName,
            AgentId = record.AgentId,
            AgentName = record.AgentName,
            CreatedAt = record.CreatedAt?.ToLocalTime() ?? DateTime.UtcNow,
            Category = record.Category
        };
    }

    private static TicketStatus ParseStatus(string? value)
    {
        if (Enum.TryParse<TicketStatus>(value, true, out var parsed))
        {
            return parsed;
        }

        return TicketStatus.Open;
    }

    private static TicketPriority ParsePriority(string? value)
    {
        if (Enum.TryParse<TicketPriority>(value, true, out var parsed))
        {
            return parsed;
        }

        return TicketPriority.Medium;
    }

    private static Guid? ParseGuid(string? value)
    {
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        return null;
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
                _logger.LogDebug(ex, "Failed to remove support ticket realtime handler.");
            }
        }

        _realtimeChannel = null;
        _changeHandler = null;
        _subscriptionInitialized = false;

        return ValueTask.CompletedTask;
    }
}

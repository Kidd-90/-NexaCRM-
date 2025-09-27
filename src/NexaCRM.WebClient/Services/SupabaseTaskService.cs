using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.Enums;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using CrmTask = NexaCRM.UI.Models.Task;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using RealtimeEventType = Supabase.Realtime.Constants.EventType;
using RealtimeListenType = Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType;
using Supabase.Realtime.Interfaces;
using Supabase.Realtime.PostgresChanges;
using NexaCRM.Service.Supabase;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseTaskService : ITaskService, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseTaskService> _logger;
    private readonly Dictionary<int, CrmTask> _cache = new();
    private readonly object _syncRoot = new();
    private readonly SemaphoreSlim _subscriptionLock = new(1, 1);
    private bool _subscriptionInitialized;
    private IRealtimeChannel? _realtimeChannel;
    private IRealtimeChannel.PostgresChangesHandler? _changeHandler;

    public event Action<CrmTask>? TaskUpserted;
    public event Action<int>? TaskDeleted;

    public SupabaseTaskService(SupabaseClientProvider clientProvider, ILogger<SupabaseTaskService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<CrmTask>> GetTasksAsync()
    {
        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TaskRecord>()
                .Order(x => x.DueDate, PostgrestOrdering.Ascending)
                .Get();

            var models = response.Models ?? new List<TaskRecord>();
            var tasks = models.Select(MapToTask).ToList();

            lock (_syncRoot)
            {
                _cache.Clear();
                foreach (var task in tasks)
                {
                    _cache[task.Id] = task;
                }
            }

            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tasks from Supabase.");
            throw;
        }
    }

    public async Task<CrmTask?> GetTaskByIdAsync(int id)
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
            var response = await client.From<TaskRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            if (record is null)
            {
                return null;
            }

            var task = MapToTask(record);
            lock (_syncRoot)
            {
                _cache[task.Id] = task;
            }

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load task {TaskId} from Supabase.", id);
            throw;
        }
    }

    public async Task CreateTaskAsync(CrmTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            var currentUserId = client.Auth.CurrentUser?.Id;

            var record = new TaskRecord
            {
                Title = task.Title ?? string.Empty,
                Description = task.Description,
                DueDate = task.DueDate == default ? null : task.DueDate,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority.ToString(),
                AssignedTo = ParseGuid(task.AssignedTo),
                AssignedToName = task.AssignedTo,
                CreatedBy = ParseGuid(currentUserId)
            };

            var response = await client.From<TaskRecord>().Insert(record);
            var insertedRecord = response.Models.FirstOrDefault();
            if (insertedRecord is not null)
            {
                var createdTask = MapToTask(insertedRecord);
                lock (_syncRoot)
                {
                    _cache[createdTask.Id] = createdTask;
                }

                TaskUpserted?.Invoke(createdTask);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create task in Supabase.");
            throw;
        }
    }

    public async Task UpdateTaskAsync(CrmTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            var record = new TaskRecord
            {
                Id = task.Id,
                Title = task.Title ?? string.Empty,
                Description = task.Description,
                DueDate = task.DueDate == default ? null : task.DueDate,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority.ToString(),
                AssignedTo = ParseGuid(task.AssignedTo),
                AssignedToName = task.AssignedTo
            };

            var response = await client.From<TaskRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, task.Id)
                .Update(record);

            var updatedRecord = response.Models.FirstOrDefault() ?? record;
            var updatedTask = MapToTask(updatedRecord);

            lock (_syncRoot)
            {
                _cache[updatedTask.Id] = updatedTask;
            }

            TaskUpserted?.Invoke(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update task {TaskId} in Supabase.", task.Id);
            throw;
        }
    }

    public async Task DeleteTaskAsync(int id)
    {
        try
        {
            await EnsureRealtimeSubscriptionAsync();
            var client = await _clientProvider.GetClientAsync();
            await client.From<TaskRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();

            var removed = false;
            lock (_syncRoot)
            {
                removed = _cache.Remove(id);
            }

            if (removed)
            {
                TaskDeleted?.Invoke(id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task {TaskId} from Supabase.", id);
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
            _realtimeChannel = await client.From<TaskRecord>()
                .On(RealtimeListenType.All, _changeHandler);

            _subscriptionInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to subscribe to Supabase task realtime channel.");
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
                    var record = change.Model<TaskRecord>();
                    if (record is null)
                    {
                        return;
                    }

                    var task = MapToTask(record);
                    lock (_syncRoot)
                    {
                        _cache[task.Id] = task;
                    }

                    TaskUpserted?.Invoke(task);
                    break;

                case RealtimeEventType.Delete:
                    HandleTaskDeleted(change);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process Supabase task realtime payload.");
        }
    }

    private void HandleTaskDeleted(PostgresChangesResponse change)
    {
        int? taskId = change.OldModel<TaskRecord>()?.Id;

        if (taskId is null)
        {
            return;
        }

        var removed = false;
        lock (_syncRoot)
        {
            removed = _cache.Remove(taskId.Value);
        }

        if (removed)
        {
            TaskDeleted?.Invoke(taskId.Value);
        }
    }

    private static CrmTask MapToTask(TaskRecord record)
    {
        return new CrmTask
        {
            Id = record.Id,
            Title = record.Title,
            Description = record.Description,
            DueDate = record.DueDate ?? DateTime.Today,
            IsCompleted = record.IsCompleted,
            Priority = ParsePriority(record.Priority),
            AssignedTo = record.AssignedToName ?? record.AssignedTo?.ToString()
        };
    }

    private static Priority ParsePriority(string? value)
    {
        if (Enum.TryParse<Priority>(value, true, out var parsed))
        {
            return parsed;
        }

        return Priority.Medium;
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
                _logger.LogDebug(ex, "Failed to remove Supabase task realtime handler.");
            }
        }

        _realtimeChannel = null;
        _changeHandler = null;
        _subscriptionInitialized = false;

        return ValueTask.CompletedTask;
    }
}

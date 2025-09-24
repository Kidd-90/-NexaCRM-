using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using CrmTask = NexaCRM.WebClient.Models.Task;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseTaskService : ITaskService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseTaskService> _logger;

    public SupabaseTaskService(SupabaseClientProvider clientProvider, ILogger<SupabaseTaskService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<CrmTask>> GetTasksAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TaskRecord>()
                .Order(x => x.DueDate, PostgrestOrdering.Ascending)
                .Get();

            var models = response.Models ?? new List<TaskRecord>();
            if (models.Count == 0)
            {
                return new List<CrmTask>();
            }

            return models.Select(MapToTask).ToList()!;
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
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<TaskRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            return record is null ? null : MapToTask(record);
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

            await client.From<TaskRecord>().Insert(record);
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

            await client.From<TaskRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, task.Id)
                .Update(record);
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
            var client = await _clientProvider.GetClientAsync();
            await client.From<TaskRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete task {TaskId} from Supabase.", id);
            throw;
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
}

using System;
using NexaCRM.UI.Models;
using System.Collections.Generic;

namespace NexaCRM.UI.Services.Interfaces
{
    public interface ITaskService
    {
        event Action<Models.Task>? TaskUpserted;
        event Action<int>? TaskDeleted;

        System.Threading.Tasks.Task<IEnumerable<Models.Task>> GetTasksAsync();
        System.Threading.Tasks.Task<Models.Task?> GetTaskByIdAsync(int id);
        System.Threading.Tasks.Task CreateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task DeleteTaskAsync(int id);
    }
}

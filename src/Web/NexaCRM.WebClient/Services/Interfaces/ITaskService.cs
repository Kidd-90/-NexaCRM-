using NexaCRM.WebClient.Models;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface ITaskService
    {
        System.Threading.Tasks.Task<IEnumerable<Models.Task>> GetTasksAsync();
        System.Threading.Tasks.Task<Models.Task?> GetTaskByIdAsync(int id);
        System.Threading.Tasks.Task CreateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task DeleteTaskAsync(int id);
    }
}

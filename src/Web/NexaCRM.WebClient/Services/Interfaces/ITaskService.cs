using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<Models.Task>> GetTasksAsync();
        Task<Models.Task> GetTaskByIdAsync(int id);
        Task CreateTaskAsync(Models.Task task);
        Task UpdateTaskAsync(Models.Task task);
        Task DeleteTaskAsync(int id);
    }
}

using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Enums;
using NexaCRM.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexaCRM.UI.Services.Mock
{
    public class MockTaskService : ITaskService
    {
        private readonly List<Models.Task> _tasks;

        public event Action<Models.Task>? TaskUpserted;
        public event Action<int>? TaskDeleted;

        public MockTaskService()
        {
            _tasks = new List<Models.Task>
            {
                new Models.Task { Id = 1, Title = "Follow up with John Doe", Description = "Discuss the new proposal.", DueDate = DateTime.Now.AddDays(2), IsCompleted = false, Priority = Priority.High, AssignedTo = "Alice" },
                new Models.Task { Id = 2, Title = "Prepare presentation for marketing meeting", Description = "Include Q2 results and Q3 forecast.", DueDate = DateTime.Now.AddDays(5), IsCompleted = false, Priority = Priority.Medium, AssignedTo = "Bob" },
                new Models.Task { Id = 3, Title = "Review campaign performance", Description = "Analyze the results of the summer sale campaign.", DueDate = DateTime.Now.AddDays(10), IsCompleted = true, Priority = Priority.Low, AssignedTo = "Alice" }
            };
        }

        public System.Threading.Tasks.Task<IEnumerable<Models.Task>> GetTasksAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Models.Task>>(_tasks);
        }

        public System.Threading.Tasks.Task<Models.Task?> GetTaskByIdAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public System.Threading.Tasks.Task CreateTaskAsync(Models.Task task)
        {
            task.Id = _tasks.Max(t => t.Id) + 1;
            _tasks.Add(task);
            TaskUpserted?.Invoke(task);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task UpdateTaskAsync(Models.Task task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.DueDate = task.DueDate;
                existingTask.IsCompleted = task.IsCompleted;
                existingTask.Priority = task.Priority;
                existingTask.AssignedTo = task.AssignedTo;
                TaskUpserted?.Invoke(existingTask);
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task DeleteTaskAsync(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _tasks.Remove(task);
                TaskDeleted?.Invoke(id);
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}

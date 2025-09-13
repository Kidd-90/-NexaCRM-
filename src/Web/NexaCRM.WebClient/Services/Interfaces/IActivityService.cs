using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetActivitiesByContactIdAsync(int contactId);
        Task AddActivityAsync(Activity activity);
    }
}

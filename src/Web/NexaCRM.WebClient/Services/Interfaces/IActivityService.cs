using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetActivitiesByContactIdAsync(int contactId);
        Task AddActivityAsync(Activity activity);
    }
}

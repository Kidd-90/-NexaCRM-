using NexaCRM.UI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.UI.Services.Interfaces
{
    public interface IAgentService
    {
        Task<IEnumerable<Agent>> GetAgentsAsync();
    }
}

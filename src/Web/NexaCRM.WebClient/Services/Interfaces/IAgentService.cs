using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IAgentService
    {
        Task<IEnumerable<Agent>> GetAgentsAsync();
    }
}

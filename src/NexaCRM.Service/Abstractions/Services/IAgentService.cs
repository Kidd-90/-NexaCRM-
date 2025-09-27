using NexaCRM.UI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentModel = NexaCRM.Services.Admin.Models.Agent;

namespace NexaCRM.UI.Services.Interfaces
{
    public interface IAgentService
    {
        Task<IEnumerable<AgentModel>> GetAgentsAsync();
    }
}

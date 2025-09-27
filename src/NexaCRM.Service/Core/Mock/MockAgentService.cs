using NexaCRM.UI.Models;
using NexaCRM.UI.Services.Interfaces;
using System.Collections.Generic;
using AgentModel = NexaCRM.Services.Admin.Models.Agent;

namespace NexaCRM.UI.Services.Mock
{
    public class MockAgentService : IAgentService
    {
        private readonly List<AgentModel> _agents;

        public MockAgentService()
        {
            _agents = new List<AgentModel>
            {
                new AgentModel { Id = 1, Name = "Ethan Harper", Email = "ethan.harper@example.com", Role = "Sales" },
                new AgentModel { Id = 2, Name = "Olivia Bennett", Email = "olivia.bennett@example.com", Role = "Sales" },
                new AgentModel { Id = 3, Name = "Noah Foster", Email = "noah.foster@example.com", Role = "Sales" },
                new AgentModel { Id = 4, Name = "Isabella Reed", Email = "isabella.reed@example.com", Role = "Sales" },
                new AgentModel { Id = 5, Name = "Lucas Coleman", Email = "lucas.coleman@example.com", Role = "Sales" },
                new AgentModel { Id = 6, Name = "Mia Manager", Email = "mia.manager@example.com", Role = "Manager" }
            };
        }

        public System.Threading.Tasks.Task<IEnumerable<AgentModel>> GetAgentsAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<AgentModel>>(_agents);
        }
    }
}

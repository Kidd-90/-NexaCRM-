using NexaCRM.UI.Models;
using NexaCRM.UI.Services.Interfaces;
using System.Collections.Generic;

namespace NexaCRM.UI.Services.Mock
{
    public class MockAgentService : IAgentService
    {
        private readonly List<Agent> _agents;

        public MockAgentService()
        {
            _agents = new List<Agent>
            {
                new Agent { Id = 1, Name = "Ethan Harper", Email = "ethan.harper@example.com", Role = "Sales" },
                new Agent { Id = 2, Name = "Olivia Bennett", Email = "olivia.bennett@example.com", Role = "Sales" },
                new Agent { Id = 3, Name = "Noah Foster", Email = "noah.foster@example.com", Role = "Sales" },
                new Agent { Id = 4, Name = "Isabella Reed", Email = "isabella.reed@example.com", Role = "Sales" },
                new Agent { Id = 5, Name = "Lucas Coleman", Email = "lucas.coleman@example.com", Role = "Sales" },
                new Agent { Id = 6, Name = "Mia Manager", Email = "mia.manager@example.com", Role = "Manager" }
            };
        }

        public System.Threading.Tasks.Task<IEnumerable<Agent>> GetAgentsAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Agent>>(_agents);
        }
    }
}

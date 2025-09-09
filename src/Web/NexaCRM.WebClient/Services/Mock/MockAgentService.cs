using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockAgentService : IAgentService
    {
        private readonly List<Agent> _agents;

        public MockAgentService()
        {
            _agents = new List<Agent>
            {
                new Agent { Id = 1, Name = "Ethan Harper", Email = "ethan.harper@example.com" },
                new Agent { Id = 2, Name = "Olivia Bennett", Email = "olivia.bennett@example.com" },
                new Agent { Id = 3, Name = "Noah Foster", Email = "noah.foster@example.com" },
                new Agent { Id = 4, Name = "Isabella Reed", Email = "isabella.reed@example.com" },
                new Agent { Id = 5, Name = "Lucas Coleman", Email = "lucas.coleman@example.com" }
            };
        }

        public System.Threading.Tasks.Task<IEnumerable<Agent>> GetAgentsAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Agent>>(_agents);
        }
    }
}

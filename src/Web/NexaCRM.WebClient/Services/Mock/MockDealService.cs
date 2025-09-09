using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockDealService : IDealService
    {
        public System.Threading.Tasks.Task<IEnumerable<Deal>> GetDealsAsync()
        {
            var deals = new List<Deal>
            {
                new Deal { Id = 1, Name = "Deal 1", Stage = "Prospecting", Amount = 50000, Company = "Company A", ContactPerson = "John Doe" },
                new Deal { Id = 2, Name = "Deal 2", Stage = "Qualification", Amount = 30000, Company = "Company B", ContactPerson = "Jane Smith" },
                new Deal { Id = 3, Name = "Deal 3", Stage = "Proposal", Amount = 20000, Company = "Company C", ContactPerson = "Peter Jones" },
                new Deal { Id = 4, Name = "Deal 4", Stage = "Negotiation", Amount = 15000, Company = "Company D", ContactPerson = "Mary Johnson" },
                new Deal { Id = 5, Name = "Deal 5", Stage = "Closed (Won)", Amount = 10000, Company = "Company E", ContactPerson = "David Williams" }
            };
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Deal>>(deals);
        }
    }
}

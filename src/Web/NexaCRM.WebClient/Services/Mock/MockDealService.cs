using System;
using System.Collections.Generic;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockDealService : IDealService
    {
        public System.Threading.Tasks.Task<IEnumerable<Deal>> GetDealsAsync()
        {
            var deals = new List<Deal>
            {
                new Deal { Id = 1, Name = "Deal 1", Stage = "Prospecting", Amount = 50000, Company = "Company A", ContactPerson = "John Doe", Owner = "Alex Kim", CreatedDate = DateTime.Today.AddDays(-3) },
                new Deal { Id = 2, Name = "Deal 2", Stage = "Qualification", Amount = 30000, Company = "Company B", ContactPerson = "Jane Smith", Owner = "Alex Kim", CreatedDate = DateTime.Today.AddDays(-18) },
                new Deal { Id = 3, Name = "Deal 3", Stage = "Proposal", Amount = 20000, Company = "Company C", ContactPerson = "Peter Jones", Owner = "Morgan Lee", CreatedDate = DateTime.Today.AddDays(-33) },
                new Deal { Id = 4, Name = "Deal 4", Stage = "Negotiation", Amount = 15000, Company = "Company D", ContactPerson = "Mary Johnson", Owner = "Jordan Park", CreatedDate = DateTime.Today.AddDays(-8) },
                new Deal { Id = 5, Name = "Deal 5", Stage = "Closed (Won)", Amount = 100000, Company = "Company E", ContactPerson = "David Williams", Owner = "Morgan Lee", CreatedDate = DateTime.Today.AddDays(-65) }
            };
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Deal>>(deals);
        }
    }
}

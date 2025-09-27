using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NexaCRM.UI.Models;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock
{
    public class MockDealService : IDealService
    {
        private static readonly List<DealStage> DealStages = new()
        {
            new DealStage { Id = 1, Name = "Prospecting", SortOrder = 1 },
            new DealStage { Id = 2, Name = "Qualification", SortOrder = 2 },
            new DealStage { Id = 3, Name = "Proposal", SortOrder = 3 },
            new DealStage { Id = 4, Name = "Negotiation", SortOrder = 4 },
            new DealStage { Id = 5, Name = "Closed (Won)", SortOrder = 5 },
            new DealStage { Id = 6, Name = "Closed (Lost)", SortOrder = 6 }
        };

        private static readonly List<Deal> Deals = new()
        {
            new Deal { Id = 1, Name = "Deal 1", Stage = "Prospecting", Amount = 50000, Company = "Company A", ContactPerson = "John Doe", Owner = "Alex Kim", CreatedDate = DateTime.Today.AddDays(-3) },
            new Deal { Id = 2, Name = "Deal 2", Stage = "Qualification", Amount = 30000, Company = "Company B", ContactPerson = "Jane Smith", Owner = "Alex Kim", CreatedDate = DateTime.Today.AddDays(-18) },
            new Deal { Id = 3, Name = "Deal 3", Stage = "Proposal", Amount = 20000, Company = "Company C", ContactPerson = "Peter Jones", Owner = "Morgan Lee", CreatedDate = DateTime.Today.AddDays(-33) },
            new Deal { Id = 4, Name = "Deal 4", Stage = "Negotiation", Amount = 15000, Company = "Company D", ContactPerson = "Mary Johnson", Owner = "Jordan Park", CreatedDate = DateTime.Today.AddDays(-8) },
            new Deal { Id = 5, Name = "Deal 5", Stage = "Closed (Won)", Amount = 100000, Company = "Company E", ContactPerson = "David Williams", Owner = "Morgan Lee", CreatedDate = DateTime.Today.AddDays(-65) }
        };

        public System.Threading.Tasks.Task<IEnumerable<Deal>> GetDealsAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Deal>>(Deals.ToList());
        }

        public System.Threading.Tasks.Task<IReadOnlyList<DealStage>> GetDealStagesAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<DealStage> ordered = DealStages
                .OrderBy(stage => stage.SortOrder)
                .ThenBy(stage => stage.Name)
                .Select(stage => new DealStage
                {
                    Id = stage.Id,
                    Name = stage.Name,
                    SortOrder = stage.SortOrder
                })
                .ToList();

            return System.Threading.Tasks.Task.FromResult(ordered);
        }

        public System.Threading.Tasks.Task<Deal> CreateDealAsync(DealCreateRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var stage = DealStages.FirstOrDefault(s => s.Id == request.StageId);
            if (stage is null)
            {
                throw new InvalidOperationException($"Unknown stage id {request.StageId}.");
            }

            var nextId = Deals.Count == 0 ? 1 : Deals.Max(d => d.Id) + 1;
            var deal = new Deal
            {
                Id = nextId,
                Name = request.Name,
                Stage = stage.Name,
                Amount = request.Amount,
                Company = request.Company,
                ContactPerson = request.ContactName,
                Owner = request.Owner,
                CreatedDate = DateTime.UtcNow
            };

            Deals.Add(deal);
            return System.Threading.Tasks.Task.FromResult(deal);
        }
    }
}
